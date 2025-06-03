using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponSelectionData", menuName = "ScriptableObjects/WeaponSelectionData", order = 1)]
public class WeaponSelectionData : ScriptableObject
{
    public List<WeaponSlotData> weaponSlots = new List<WeaponSlotData>();
}
