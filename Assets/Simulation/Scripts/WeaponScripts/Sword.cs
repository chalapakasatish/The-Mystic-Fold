using UnityEngine;

public class Sword : OrigamiSwipe
{
    public override void Start()
    {
        base.Start(); // Call base logic (initializes everything)
        Debug.Log("SimpleGun prefab initialized!");
        // Add any custom setup for SimpleGun here
    }

    public override void PaperNextStep()
    {
        base.PaperNextStep(); // Reuse base step logic

        // Optional: Add extra behavior for SimpleGun
        Debug.Log("SimpleGun moved to next paper step: " + paperCount);
    }
}
