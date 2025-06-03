using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponSlotData
{
    public Sprite backgroundSprite;
    public Sprite selectedTickSprite;
    public Sprite weaponSprite;
    public string weaponPrefabPath; // New! Path to the prefab inside "Resources"
    public string weaponName;
}
