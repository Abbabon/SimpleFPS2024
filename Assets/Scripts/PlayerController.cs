using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PlayerController : Singleton<PlayerController>
{
    [Header("References")]
    [SerializeField] private Transform _playerBody;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Animator _animator;
    [SerializeField] private List<GameObject> _muzzleFlashTypes;
    [SerializeField] private Transform _muzzleFlashParent;
    [SerializeField] private Transform _firingLocation;
    
    [Header("Parameters")]
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _moveSensitivity = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private float _jumpHeight = 3f;
    
    [Header("GunParameters")]
    [SerializeField] private float _fireCooldownTime = 0.2f;
    [Tooltip("Furthest distance bullet will look for target")]
    [SerializeField] private float _maxDistance = 1000000;
    [Tooltip("Prefab of wall damange hit. The object needs 'LevelPart' tag to create decal on it.")]
    [SerializeField] private GameObject _decalHitWall;
    [Tooltip("Decal will need to be sligtly infront of the wall so it doesnt cause rendeing problems so for best feel put from 0.01-0.1.")]
    [SerializeField] private float _floatInfrontOfWall = 0.01f;
    [Tooltip("Blood prefab particle this bullet will create upoon hitting enemy")]
    [SerializeField] private GameObject _bloodEffect;
    [Tooltip("Put Weapon layer and Player layer to ignore bullet raycast.")]
    [SerializeField] private LayerMask _ignoredLayers;
    
    private float _xAxisRotation = 0f;
    private Vector3 _velocity;
    private bool _isGrounded;
    private Coroutine _fireCooldown;
    private WaitForSeconds _cooldownWaitForSeconds;
    private bool _currentCusrorState;
    private bool _playerWalking;
    private RaycastHit hit;

    private void Start()
    {
        SetCursorState(true);
        _cooldownWaitForSeconds = new WaitForSeconds(_fireCooldownTime);
        AudioController.Instance.PlaySound(SoundType.PullWeapon);
    }

    private void SetCursorState(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
        _currentCusrorState = state;
    }

    void Update()
    {
        CheckCursorState();
        CheckGrounded();
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleGravity();
        HandleFireButton();
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        _animator.SetFloat("walkSpeed", _playerWalking ? 2f : 0f);
        //TODO: implement aiming
    }

    private void CheckCursorState()
    {
        if (_currentCusrorState && Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(false);
        }
        
        if (!_currentCusrorState && Input.GetMouseButtonDown(0))
        {
            SetCursorState(true);
        }
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.Raycast(_groundCheck.position, _playerBody.up * -1, _groundDistance, _groundMask);
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
    }

    private void HandleLook()
    {
        if (!_currentCusrorState)
            return;
        
        var xMouseDelta = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        var yMouseDelta = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        
        _playerBody.Rotate(Vector3.up * xMouseDelta);
        
        _xAxisRotation -= yMouseDelta;
        _xAxisRotation = Mathf.Clamp(_xAxisRotation, -90f, 90f);
        _playerCamera.localRotation = Quaternion.Euler(_xAxisRotation, 0f, 0f);
    }
    
    private void HandleMovement()
    {
        _playerWalking = false;
        var xDiff = _playerCamera.right * (Input.GetAxis("Horizontal") * _moveSensitivity * Time.deltaTime);
        var zDiff = _playerCamera.forward * (Input.GetAxis("Vertical") * _moveSensitivity * Time.deltaTime);

        var movementDiff = xDiff + zDiff;
        _characterController.Move(movementDiff);
        _playerWalking = movementDiff.magnitude > 0;
    }
    
    private void HandleJump()
    {
        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            AudioController.Instance.PlaySound(SoundType.Jump);
        }
    }
    
    private void HandleGravity()
    {
        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(0.5f * _velocity * Time.deltaTime);
    }
    
    private void HandleFireButton()
    {
        if (Input.GetButtonDown("Fire1") && _fireCooldown == null)
        {
            Fire();
            _fireCooldown = StartCoroutine(FireCooldown());
        }
    }

    private void Fire()
    {
        AudioController.Instance.PlaySound(SoundType.Shoot);
        HandleRaycastBullet();
        HandleRecoil();
        HandleMuzzleFlash();
    }

    private void HandleMuzzleFlash()
    {
        //TODO: object pool!
        var prefab = _muzzleFlashTypes[UnityEngine.Random.Range(0, _muzzleFlashTypes.Count)];
        var muzzleFlash = Instantiate(prefab, _muzzleFlashParent.transform.position,
            _muzzleFlashParent.transform.rotation * Quaternion.Euler(0, 0, 90));
        muzzleFlash.transform.parent = _muzzleFlashParent.transform;
    }

    private void HandleRecoil()
    {
        // TODO: Camera shake on recoil
    }

    private void HandleRaycastBullet()
    {
        if (!Physics.Raycast(_firingLocation.position, _firingLocation.forward, out hit, _maxDistance, ~_ignoredLayers)) 
            return;
        
        if (!_decalHitWall) 
            return;
     
        // TODO: object pools!
        if(hit.transform.CompareTag("Geometry"))
        {
            Instantiate(_decalHitWall, hit.point + hit.normal * _floatInfrontOfWall, Quaternion.LookRotation(hit.normal));
        }
        
        if(hit.transform.CompareTag("Enemy"))
        {
            Instantiate(_bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
            //TODO: extract to game manager, multiple HP damage
            hit.transform.GetComponent<Enemy>()?.TakeDamage(1);
        }
        AudioController.Instance.PlaySound(SoundType.Hit);
    }

    private IEnumerator FireCooldown()
    {
        yield return _cooldownWaitForSeconds;
        _fireCooldown = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_firingLocation.position, _firingLocation.forward * _maxDistance);
    }
}
