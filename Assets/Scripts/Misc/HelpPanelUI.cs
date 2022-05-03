using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject helpMessage;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
            helpMessage.SetActive(!helpMessage.activeSelf);
        }
    }
}
