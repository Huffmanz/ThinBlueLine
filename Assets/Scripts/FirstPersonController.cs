using System;
using System.Collections;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public static FirstPersonController Instance;

    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool enableHeadbob = true;
    [SerializeField] private bool slideOnSlopes = true;
    [SerializeField] private bool canLean = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private MouseButton aimKey = MouseButton.Right;
    [SerializeField] private KeyCode leanLeft = KeyCode.Q;
    [SerializeField] private KeyCode leanRight = KeyCode.E;


    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float slopeSpeed = 8f;
    private float targetSpeed;
    private Vector3 moveDirection;
    private Vector2 currentInput;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    private float rotationX = 0;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Leaning")]
    [SerializeField] private Transform leanPivot;
    [SerializeField] private float leanDegrees;
    [SerializeField] private float leanSmoothing;
    [SerializeField] private float leanDistance;

    private Vector3 leanPivotStartingLocalPos;
    private Vector3 targetLeanPivotPos;
    private Vector3 currentLeanPivotPos;
    private Vector3 leanPivotVelocity;
    private float leanVelocity;
    private float targetLean;
    private float currentLean;
    private bool isLeaning = false;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14.0f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18.0f;
    [SerializeField] private float sprintBobAmount = .11f;
    [SerializeField] private float crouchBobSpeed = 8.0f;
    [SerializeField] private float crouchBobAmount = 0.025f;

    [Header("Weapon")]
    [SerializeField] private WeaponController weaponController;

    private Animator animator;
    private int speedString = Animator.StringToHash("Speed");
    private int shootString = Animator.StringToHash("Shoot");
    private int reloadString = Animator.StringToHash("Reload");
    private int dryFireString = Animator.StringToHash("DryFire");


    private float defaultYPos = 0;
    private float headBobTimer;
    private bool aiming;

    public Camera playerCamera { get; private set; }
    private CharacterController characterController;

    private Vector3 hitPointNormal;
    private bool isSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, characterController.height))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultYPos = playerCamera.transform.localPosition.y;
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        weaponController.WeaponFiredEvent += HandleShooting;
        weaponController.WeaponDryFireEvent += HandleDryFire;
        weaponController.WeaponReloadEvent += HandleReload;
        leanPivotStartingLocalPos = leanPivot.localPosition;
    }

    private void Update()
    {
        if (!CanMove) return;
        HandleMovementInput();
        HandleMouseLook();
        HandleJump();
        HandleCrouch();
        HandleLean();
        HandleHeadbob();
        HandleAiming();
        ApplyFinalMovements();
    }

    private void HandleMovementInput()
    {
        targetSpeed = IsSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : walkSpeed);
        currentInput = new Vector2(targetSpeed * Input.GetAxis("Vertical"), targetSpeed * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (slideOnSlopes && isSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
        if (aiming)
        {
            animator.SetFloat(speedString, 0);
        }
        else
        {
            animator.SetFloat(speedString, currentInput.magnitude);
        }
    }

    private void HandleJump()
    {
        if (!canJump) return;
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if (!canCrouch) return;
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, standingHeight)) return;
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;
        isCrouching = !isCrouching;
        duringCrouchAnimation = false;
    }

    private void HandleLean()
    {
        if (!canLean) return;

        if (Input.GetKeyDown(leanLeft))
        {
            targetLean = isLeaning ? 0 : leanDegrees;
            isLeaning = !isLeaning;
            if (isLeaning)
            {
                targetLeanPivotPos = leanPivotStartingLocalPos;
                targetLeanPivotPos.x -= leanDistance;
            }
            else
            {
                targetLeanPivotPos = leanPivotStartingLocalPos;
            }
        }
        else if (Input.GetKeyDown(leanRight))
        {
            targetLean = isLeaning ? 0 : -leanDegrees;
            isLeaning = !isLeaning;
            if (isLeaning)
            {
                targetLeanPivotPos = leanPivotStartingLocalPos;
                targetLeanPivotPos.x += leanDistance;
            }
            else
            {
                targetLeanPivotPos = leanPivotStartingLocalPos;
            }
        }

        if (IsSprinting)
        {
            targetLean = 0;
        }

        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);
        leanPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
        currentLeanPivotPos = Vector3.SmoothDamp(currentLeanPivotPos, targetLeanPivotPos, ref leanPivotVelocity, leanSmoothing);
        leanPivot.localPosition = currentLeanPivotPos;
    }


    private void HandleHeadbob()
    {
        if (!enableHeadbob) return;
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > .1f || Mathf.Abs(moveDirection.z) > .1f)
        {
            float bobSpeed = isCrouching ? crouchBobSpeed : (IsSprinting ? sprintBobSpeed : walkBobSpeed);
            float bobAmount = isCrouching ? crouchBobAmount : (IsSprinting ? sprintBobAmount : walkBobAmount);
            headBobTimer += Time.deltaTime * bobSpeed;
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(headBobTimer) * bobAmount,
                playerCamera.transform.localPosition.z
            );
        }

    }

    private void HandleAiming()
    {
        if (!aiming && Input.GetMouseButtonDown(1))
        {
            AimingInPressed();
        }

        if (aiming && !Input.GetMouseButton(1))
        {
            AimingInReleased();
        }

        if (aiming && IsSprinting)
        {
            AimingInReleased();
        }

        CalculateAimingIn();
    }

    private void AimingInPressed()
    {
        aiming = true;
    }

    private void AimingInReleased()
    {
        aiming = false;
    }

    private void CalculateAimingIn()
    {
        if (!weaponController) return;
        weaponController.IsAiming = aiming;
    }

    private void HandleShooting()
    {
        animator.SetTrigger(shootString);
    }

    private void HandleDryFire()
    {
        animator.SetTrigger(dryFireString);
    }

    private void HandleReload()
    {
        animator.SetTrigger(reloadString);
    }
}
