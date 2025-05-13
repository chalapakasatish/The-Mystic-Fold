using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using TMPro;

public class OrigamiSwipe : MonoBehaviour
{
    public enum SwipeDirection { Right, Left, Up, Down, Any }

    [System.Serializable]
    public class AnimationStep
    {
        public string animationName;
        public SwipeDirection swipeDirection;
        public float inventoryScale = 0f;
        public bool sendToInventory = true;
        public bool hideIfNotSent = true;
        public Vector3 inventoryOffset = Vector3.zero;
        public Vector3 positionOffset = Vector3.zero;
    }

    [Header("Paper Settings")]
    public List<GameObject> paperAnimators;
    public List<AnimationStep> animationSteps;
    public float swipeSpeed = 0.001f;

    [Header("Inventory Settings")]
    public Vector3 inventoryStartPosition = new(-5f, 4f, 0f);
    public Vector2 gridSpacing = new(1f, 1f);
    public int itemsPerRow = 5;
    public float defaultInventoryScale = 0.2f;

    [Header("Final Object Settings")]
    public GameObject finalObjectPrefab;
    public ParticleSystem boomExplosionParticle;

    [Header("Cinemachine Camera Settings")]
    public Vector3 cameraFinalPosition;
    public Vector3 cameraFinalRotation;
    public Vector3 cameraZoomInPosition;
    public Vector3 cameraZoomInRotation;

    [Header("UI Settings")]
    public Slider scrubProgressBar;
    public Canvas uiCanvas;

    [Header("Scrub Bar Flash Settings")]
    public Color normalColor = Color.white;
    public Color flashColor = Color.green;
    public float flashDuration = 0.3f;

    [Header("Tutorial Arrow Settings")]
    public RectTransform tutorialArrow;
    public Vector2 tutorialArrowPosition;
    public float moveDistance = 100f;
    public float moveDuration = 1f;

    // internal
    private Image scrubBarImage;
    private Tween scrubTween;
    private Tween arrowTween;
    public List<GameObject> collectedPieces;
    protected int paperCount = 0;
    private int currentIndex = 0;
    private bool isSwiping = false;
    private bool canSwipe = true;
    private Vector2 lastPos;
    private Animator animator;
    private bool hasSpawnedFinalObject = false;
    private float scrubTime = 0f;
    private Vector3 arrowInitialPos;
    private bool arrowMoving = false;
    public TextMeshProUGUI ProgressBarText;
    public virtual void Start()
    {
        if (tutorialArrow != null)
            tutorialArrow.gameObject.SetActive(false);

        if (scrubProgressBar != null)
        {
            scrubBarImage = scrubProgressBar.fillRect.GetComponent<Image>();
            scrubBarImage.color = normalColor;
        }

        PaperNextStep();
    }

    private void Update()
    {
#if UNITY_EDITOR
        bool began = Input.GetMouseButtonDown(0);
        bool moved = Input.GetMouseButton(0);
        bool ended = Input.GetMouseButtonUp(0);
        Vector2 pos = Input.mousePosition;
#else
        bool began = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool moved = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved;
        bool ended = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
        Vector2 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#endif

        if (began && tutorialArrow != null && tutorialArrow.gameObject.activeSelf)
        {
            tutorialArrow.gameObject.SetActive(false);
            if (arrowTween != null) arrowTween.Kill();
            arrowMoving = false;
        }

        HandleSwipe(began, moved, ended, pos);

        if (scrubProgressBar != null)
        {
            if (scrubTween != null && scrubTween.IsActive()) scrubTween.Kill();
            scrubTween = scrubProgressBar.DOValue(scrubTime, 0.2f);
        }
    }

    private void HandleSwipe(bool began, bool moved, bool ended, Vector2 inputPos)
    {
        if (began && canSwipe)
        {
            lastPos = inputPos;
            isSwiping = true;
        }
        else if (moved && isSwiping && canSwipe && currentIndex < animationSteps.Count)
        {
            Vector2 delta = inputPos - lastPos;
            var step = animationSteps[currentIndex];

            float swipeAmount = step.swipeDirection == SwipeDirection.Any
                ? delta.magnitude * swipeSpeed
                : Vector2.Dot(delta.normalized, GetSwipeVector(step.swipeDirection)) * delta.magnitude * swipeSpeed;

            ProgressBarText.text = Mathf.RoundToInt(scrubProgressBar.GetComponent<Slider>().value * 100) + "%";


            scrubTime = Mathf.Clamp01(scrubTime + swipeAmount);
            animator.Play(step.animationName, 0, scrubTime);
            animator.speed = 0f;
            

            if (scrubProgressBar != null)
                scrubProgressBar.value = scrubTime;

            if (scrubTime >= 1f)
            {
                canSwipe = false;
                scrubTime = 0f;
                ProgressBarText.text = scrubTime + "%";
                if (scrubBarImage != null)
                {
                    scrubBarImage.DOColor(flashColor, flashDuration * 0.5f)
                        .OnComplete(() => scrubBarImage.DOColor(normalColor, flashDuration * 0.5f));
                }

                GameObject paper = paperAnimators[paperCount];
                if (step.sendToInventory)
                {
                    collectedPieces.Add(paper);
                    MoveToInventory(paper, collectedPieces.Count - 1, step);
                }
                else
                {
                    if (step.hideIfNotSent) 
                        paper.SetActive(false);
                    currentIndex++; 
                    paperCount++; 
                    PaperNextStep();
                }
            }

            lastPos = inputPos;
        }
        else if (ended)
        {
            isSwiping = false;
        }
    }

    public virtual void PaperNextStep()
    {
        if (paperCount >= paperAnimators.Count)
        {
            if (!hasSpawnedFinalObject && finalObjectPrefab != null)
            {
                boomExplosionParticle.Play();
                paperAnimators.ForEach(p => p.SetActive(false));
                finalObjectPrefab.SetActive(true);
                UIManager.instance.weaponSelectionPanel.transform.
                    DOLocalMove(new Vector3(0, -2000f,0), 1f).SetEase(Ease.OutSine).OnComplete(
                    ()=> UIManager.instance.battleButton.SetActive(true));
                scrubProgressBar.gameObject.SetActive(false);    
                hasSpawnedFinalObject = true;
                MoveCameraToFinalPosition();
            }
            return;
        }

        for (int i = 0; i < paperAnimators.Count; i++)
        {
            if (i == paperCount)
            {
                paperAnimators[i].SetActive(true);
            }
            else if (i > paperCount)
            {
                paperAnimators[i].SetActive(false);
            }
        }


        var step = animationSteps[paperCount];
        var paper = paperAnimators[paperCount];
        animator = paper.GetComponent<Animator>();
        canSwipe = false;
        paper.transform.DOMove(step.positionOffset, 1f).OnComplete(() => canSwipe = true);

        scrubTime = 0f;
        if (scrubProgressBar != null)
            scrubProgressBar.value = 0f;
        
        MoveCameraToZoomInPosition(step);
    }

    private void MoveToInventory(GameObject obj, int index, AnimationStep step)
    {
        int row = index / itemsPerRow;
        int col = index % itemsPerRow;
        Vector3 targetPos = inventoryStartPosition
                            + new Vector3(col * gridSpacing.x, -row * gridSpacing.y, 0f)
                            + step.inventoryOffset;
        float scale = step.inventoryScale > 0f ? step.inventoryScale : defaultInventoryScale;

        obj.transform.DOMove(targetPos, 1f);
        obj.transform.DOScale(Vector3.one * scale, 1f)
            .OnComplete(() =>
            {
                currentIndex++; 
                paperCount++; 
                PaperNextStep();
            });
    }

    private Vector2 GetSwipeVector(SwipeDirection dir) => dir switch
    {
        SwipeDirection.Right => Vector2.right,
        SwipeDirection.Left => Vector2.left,
        SwipeDirection.Up => Vector2.up,
        SwipeDirection.Down => Vector2.down,
        _ => Vector2.zero
    };

    private void MoveCameraToFinalPosition()
    {
        
        if (CameraManager.instance.virtualCamera == null) return;
        var cam = CameraManager.instance.virtualCamera.transform;
        cam.DOMove(cameraFinalPosition, 2f).SetEase(Ease.Linear);
        cam.DORotate(cameraFinalRotation, 2f).SetEase(Ease.Linear);
    }

    private void MoveCameraToZoomInPosition( AnimationStep step)
    {
        if (CameraManager.instance.virtualCamera == null) return;
        var cam = CameraManager.instance.virtualCamera.transform;
        cam.DOMove(cameraZoomInPosition, 1f).SetEase(Ease.Linear);
        cam.DORotate(cameraZoomInRotation, 1f).SetEase(Ease.Linear).OnComplete(() =>
        ShowTutorialArrow(step.swipeDirection)
        );
    }

    private void ShowTutorialArrow(SwipeDirection dir)
    {
        if (tutorialArrow == null || uiCanvas == null) return;

        if (arrowTween != null) arrowTween.Kill();

        tutorialArrow.anchoredPosition = tutorialArrowPosition;// Always center (0,0) of Canvas
        tutorialArrow.gameObject.SetActive(true);

        float angle = dir switch
        {
            SwipeDirection.Right => 0f,
            SwipeDirection.Left => 180f,
            SwipeDirection.Up => 90f,
            SwipeDirection.Down => -90f,
            _ => 0f
        };
        tutorialArrow.rotation = Quaternion.Euler(0f, 0f, angle);

        arrowMoving = true;
        MoveArrowOnce();
    }

    private void MoveArrowOnce()
    {
        if (!arrowMoving || tutorialArrow == null) return;

        arrowInitialPos = tutorialArrow.anchoredPosition;
        Vector2 endPos = arrowInitialPos;

        Vector2 moveDir = tutorialArrow.right;
        endPos += moveDir.normalized * moveDistance;

        arrowTween = tutorialArrow.DOAnchorPos(endPos, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                tutorialArrow.anchoredPosition = arrowInitialPos;
                if (arrowMoving) MoveArrowOnce();
            });
    }
}
