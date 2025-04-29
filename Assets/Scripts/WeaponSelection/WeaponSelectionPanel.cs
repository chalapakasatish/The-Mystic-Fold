using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class WeaponSelectionPanel : MonoBehaviour
{
    public WeaponSelectionData weaponSelectionData;

    public GameObject weaponSlotLayout; 

    public List<GameObject> weaponSlots;
    private List<Button> slotButtons = new List<Button>();
    private List<Image> tickImages = new List<Image>();

    private GameObject currentWeaponInstance;
    [SerializeField]private int levelNumber;

    public int LevelNumber { get => levelNumber; set => levelNumber = value; }

    private void Start()
    {
        WeaponSlotLayoutGenerate();

        WeaponSlotSelection();
    }
    private void WeaponSlotLayoutGenerate()
    {
        weaponSelectionData = Resources.Load<WeaponSelectionData>("ScriptableObjects/WeaponSelectionDataLevel" + LevelNumber);
        for (int i = 0; i < 4; i++)
        {
            GameObject WeaponSlotPrefab = Instantiate(Resources.Load<GameObject>("WeaponSlot/WeaponSlot"), Vector3.zero, Quaternion.identity);
            WeaponSlotPrefab.transform.SetParent(weaponSlotLayout.transform);
            weaponSlots.Add(WeaponSlotPrefab);
            WeaponSlotPrefab.gameObject.transform.localScale = Vector3.one;
            WeaponSlotPrefab.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = weaponSelectionData.weaponSlots[i].weaponName;
        }
    }
    public void WeaponSlotSelection()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {

            WeaponSlotData slotData = weaponSelectionData.weaponSlots[i];
            GameObject slot = weaponSlots[i];

            Image[] images = slot.GetComponentsInChildren<Image>();
            if (images.Length >= 3)
            {
                images[0].sprite = slotData.backgroundSprite;
                images[1].sprite = slotData.selectedTickSprite;
                images[2].sprite = slotData.weaponSprite;

                images[1].gameObject.SetActive(false);
                tickImages.Add(images[1]);
            }

            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => OnWeaponSlotClicked(index));
                slotButtons.Add(button);
            }
        }
    }
    private void OnWeaponSlotClicked(int index)
    {
        for (int i = 0; i < tickImages.Count; i++)
        {
            tickImages[i].gameObject.SetActive(i == index);
        }

        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
        }

        string prefabPath = weaponSelectionData.weaponSlots[index].weaponPrefabPath;
        GameObject weaponPrefab = Resources.Load<GameObject>(prefabPath);
        if (weaponPrefab != null)
        {
            currentWeaponInstance = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Weapon prefab at path '{prefabPath}' not found!");
        }
    }
}
