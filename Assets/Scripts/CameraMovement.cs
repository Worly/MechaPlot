using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    public float movementSpeed;

    [SerializeField]
    public float mouseSensitivity;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.localPosition += transform.forward * movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.localPosition -= transform.forward * movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            transform.localPosition += transform.right * movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            transform.localPosition -= transform.right * movementSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
            transform.localPosition += transform.up * movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift))
            transform.localPosition -= transform.up * movementSpeed * Time.deltaTime;

        yaw += mouseSensitivity * Input.GetAxis("Mouse X");
        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
