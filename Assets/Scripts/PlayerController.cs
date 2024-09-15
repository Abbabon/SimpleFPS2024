using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerBody;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private LayerMask _groundMask;
    
    [Header("Parameters")]
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _moveSensitivity = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _fireCooldownTime = 0.2f;
    
    private float _xAxisRotation = 0f;
    private Vector3 _velocity;
    private bool _isGrounded;
    private Coroutine _fireCooldown;
    private WaitForSeconds _cooldownWaitForSeconds;

    private void Start()
    {
        SetCursorState(true);
        _cooldownWaitForSeconds = new WaitForSeconds(_fireCooldownTime);
    }

    private void SetCursorState(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    void Update()
    {
        CheckGrounded();
        Look();
        MoveOnPlane();
        Jump();
        Gravity();
        HandleFireButton();
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.Raycast(_groundCheck.position, _playerBody.up * -1, _groundDistance, _groundMask);
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
    }

    private void Look()
    {
        var xMouseDelta = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        var yMouseDelta = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        
        _playerBody.Rotate(Vector3.up * xMouseDelta);
        
        _xAxisRotation -= yMouseDelta;
        _xAxisRotation = Mathf.Clamp(_xAxisRotation, -90f, 90f);
        _playerCamera.localRotation = Quaternion.Euler(_xAxisRotation, 0f, 0f);
    }
    
    private void MoveOnPlane()
    {
        var xDiff = _playerCamera.right * (Input.GetAxis("Horizontal") * _moveSensitivity * Time.deltaTime);
        var zDiff = _playerCamera.forward * (Input.GetAxis("Vertical") * _moveSensitivity * Time.deltaTime);

        var movementDiff = xDiff + zDiff;
        _characterController.Move(movementDiff);
    }
    
    private void Jump()
    {
        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
    }
    
    private void Gravity()
    {
        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(0.5f * _velocity * Time.deltaTime);
    }
    
    private void HandleFireButton()
    {
        if (Input.GetButtonDown("Fire1") && _fireCooldown == null)
        {
            Debug.Log("Fire!");
            _fireCooldown = StartCoroutine(FireCooldown());
        }
    }

    private IEnumerator FireCooldown()
    {
        yield return _cooldownWaitForSeconds;
        _fireCooldown = null;
    }
}
