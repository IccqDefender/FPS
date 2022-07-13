using System.Collections;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => 
        canSprint && Input.GetKey(sprintKey);

    private bool ShouldJump => 
        Input.GetKeyDown(jumpKey) && _characterController.isGrounded;

    private bool ShouldCrouch =>
        Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && _characterController.isGrounded;

    [Header("Функциональные настройки")]
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch = true;

    [Header("Управление")] 
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Настройки передвижения")] 
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;

    [Header("Настройки камеры")] 
    [SerializeField, Range(0, 10)] private float lookSpeedX = 2f;
    [SerializeField, Range(0, 10)] private float lookSpeedY = 2f;
    [SerializeField, Range(0, 180)] private float upperLookLimit = 90f;
    [SerializeField, Range(0, 180)] private float lowerLookLimit = 90f;

    [Header("Настройки прыжка")] 
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Настройки приседа")] 
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);

    private bool isCrouching;
    private bool duringCrouchAnimation;
    
    private Camera _playerCamera;
    private CharacterController _characterController;

    private Vector3 _moveDirection;
    private Vector2 _currentInput;

    private float rotationX = 0;

    //Кэширование
    private void Awake()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    //Подписка на update' методы
    private void OnEnable()
    {
        UpdateService.OnUpdate += PlayerControl;
        CharacterHealth.OnDead += KillCharacter;
    }
    private void OnDestroy()
    {
        UpdateService.OnUpdate -= PlayerControl;
        CharacterHealth.OnDead -= KillCharacter;
    }
    private void OnDisable()
    {
        UpdateService.OnUpdate -= PlayerControl;
        CharacterHealth.OnDead -= KillCharacter;
    }

    
    private void PlayerControl()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
                HandleJump();
            
            if (canCrouch)
                HandleCrouch();

            ApplyFinalMovements();
        }
    }
    private void HandleMovementInput()
    {
        _currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
            (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = _moveDirection.y;
        _moveDirection = (transform.TransformDirection(Vector3.forward) * _currentInput.x) +
                         (transform.TransformDirection(Vector3.right) * _currentInput.y);
        _moveDirection.y = moveDirectionY;
    }
    private void HandleJump()
    {
        if (ShouldJump)
            _moveDirection.y = jumpForce;
    }
    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        _playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }
    private void ApplyFinalMovements()
    {
        if (!_characterController.isGrounded)
            _moveDirection.y -= gravity * Time.deltaTime;
        _characterController.Move(_moveDirection * Time.deltaTime);
    }
    private void KillCharacter()
    {
        this.gameObject.SetActive(false);
        CanMove = false;
        CharacterHealth.canHealing = false;
    }
    
    private IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(_playerCamera.transform.position, Vector3.up, 1f))
            yield break;
        
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        var targetHeight = isCrouching ? standingHeight : crouchHeight;
        var currentHeight = _characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = _characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            _characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            _characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _characterController.height = targetHeight;
        _characterController.center = targetCenter;

        isCrouching = !isCrouching;
        
        duringCrouchAnimation = false;
    }
}
