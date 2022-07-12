using System;
using UnityEngine;


public class MouseLook : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody;

    private float _xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        UpdateService.OnUpdate += GetInput;
    }

    private void GetInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * this.mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * this.mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void OnDestroy()
    {
        UpdateService.OnUpdate -= GetInput;
    }
    private void OnDisable()
    {
        UpdateService.OnUpdate -= GetInput;
    }
}