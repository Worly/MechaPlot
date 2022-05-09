using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CameraMovement cameraMovement;

    public void Start()
    {
        Refocus();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Unfocus();
    }

    public void Unfocus()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        cameraMovement.Lock();
    }

    public void Refocus()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        cameraMovement.Unlock();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Refocus();
    }
}
