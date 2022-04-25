using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementAcceleration;
    [SerializeField] private float movementDeacceleration;
    [SerializeField] private float mouseSensitivity;

    private bool isLocked = false;
    private Vector3 currentMovementSpeed = Vector3.zero;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        if (!isLocked)
            CheckInput();

        DoMovement();
        DoDeacceleration();
    }

    void CheckInput()
    {
        if (Input.GetKey(KeyCode.W))
            currentMovementSpeed += Vector3.forward * movementAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            currentMovementSpeed -= Vector3.forward * movementAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            currentMovementSpeed += Vector3.right * movementAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            currentMovementSpeed -= Vector3.right * movementAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
            currentMovementSpeed += Vector3.up * movementAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift))
            currentMovementSpeed -= Vector3.up * movementAcceleration * Time.deltaTime;

        if (currentMovementSpeed.magnitude > movementSpeed)
            currentMovementSpeed = currentMovementSpeed.normalized * movementSpeed;

        yaw += mouseSensitivity * Input.GetAxis("Mouse X");
        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void DoMovement()
    {
        transform.position += transform.rotation * currentMovementSpeed * Time.deltaTime;
    }

    void DoDeacceleration()
    {
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)
             && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift))
        {
            if (currentMovementSpeed.magnitude < movementAcceleration * Time.deltaTime)
                currentMovementSpeed = Vector3.zero;
            else
                currentMovementSpeed -= currentMovementSpeed.normalized * movementDeacceleration * Time.deltaTime;
        }
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }
}
