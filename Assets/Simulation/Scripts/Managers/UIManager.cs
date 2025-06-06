using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject weaponSelectionPanel,bookPanel, battleButton;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void OpenBook()
    {
        bookPanel.SetActive(false);
        weaponSelectionPanel.SetActive(true);
    }
}
