using UnityEngine;

public class Heart : OrigamiSwipe
{
    public override void Start()
    {
        base.Start(); // Call base logic (initializes everything)
        Debug.Log("Axe prefab initialized!");
        // Add any custom setup for SimpleGun here
    }

    public override void PaperNextStep()
    {
        base.PaperNextStep(); // Reuse base step logic

        // Optional: Add extra behavior for SimpleGun
        Debug.Log("Axe moved to next paper step: " + paperCount);
    }
}
