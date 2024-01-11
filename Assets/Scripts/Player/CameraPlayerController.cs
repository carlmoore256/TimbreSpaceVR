using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraPlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 7.0f;
    [SerializeField]
    private float lookSpeed = 2.0f;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float yaw;
    private float pitch;
    private bool _enabled = false;

    public void EnableFPV()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _enabled = true;
    }

    public void Disable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _enabled = false;
    }


    void Update()
    {
        if(!_enabled) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Disable();
        }
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(moveHorizontal, 0, moveVertical).normalized;
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        if(!_enabled) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 targetVelocity = transform.TransformDirection(moveDirection) * moveSpeed;
        Vector3 velocityChange = targetVelocity - rb.velocity;
        velocityChange.y = 0;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(new Vector3(pitch, yaw, 0));
    }
}