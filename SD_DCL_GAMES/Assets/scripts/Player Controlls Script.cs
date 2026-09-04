using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput playerInput;

    [Header("Player")]
    [SerializeField] private int playerNumber;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float SpeedMultiplier = 2f;

    [Header("Movement Dust")]
    [SerializeField] private float dustInterval = 0.15f;
    [SerializeField] private float dustMoveThreshold = 0.01f;
    [SerializeField] private float dustSpawnBehindDistance = 0.5f;
    [SerializeField] private float dustSpawnHeight = 0.05f;

    private float nextDustTime;

    [Header("Football")]
    public LayerMask FootBallLayer;

    [SerializeField] private Transform RayPoint;
    [SerializeField] private float RayDistance = 1.1f;
    public int _kickForce = 10;
    [SerializeField] private float upAngle = 30f;
    [SerializeField] private Transform playerPositionIndicator;
    [SerializeField] private Transform playerAimIndicator;
    [SerializeField] private float _floorOffset;
    [SerializeField] private float kickRadius = 0.5f;
    [SerializeField] private float ballPushForce = 2f;

    [Header("Animation")]
    public AnimationManager animationManager;

    private bool _isRunning;
    private bool canMove = false;

    private readonly List<RaycastHit> movementHits = new List<RaycastHit>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();


        if (animationManager == null)
        {
            animationManager = GetComponent<AnimationManager>();
        }
    }

    private void Start()
    {
        rb.freezeRotation = true;

        if (animationManager != null)
        {
            animationManager.PlayIdle();
        }
    }

    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
    }

    // =========================================================
    // INPUT
    // =========================================================

    private Vector2 GetKeyboardInput()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        if (playerNumber == 1)
        {
            float x = 0f;
            float y = 0f;

            if (Keyboard.current.aKey.isPressed)
                x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                x += 1f;

            if (Keyboard.current.sKey.isPressed)
                y -= 1f;

            if (Keyboard.current.wKey.isPressed)
                y += 1f;

            return new Vector2(x, y).normalized;
        }

        if (playerNumber == 2)
        {
            float x = 0f;
            float y = 0f;

            if (Keyboard.current.jKey.isPressed)
                x -= 1f;

            if (Keyboard.current.lKey.isPressed)
                x += 1f;

            if (Keyboard.current.kKey.isPressed)
                y -= 1f;

            if (Keyboard.current.iKey.isPressed)
                y += 1f;

            return new Vector2(x, y).normalized;
        }

        return Vector2.zero;
    }

    private Vector2 GetGamepadInput()
    {
        if (playerInput == null)
            return Vector2.zero;

        if (!playerInput.actions.enabled)
            return Vector2.zero;

        return playerInput.actions["Move"].ReadValue<Vector2>();
    }

    private Vector2 GetMovementInput()
    {
        Vector2 keyboardInput = GetKeyboardInput();

        // Keyboard takes priority if it is being used.
        if (keyboardInput.sqrMagnitude > 0.001f)
            return keyboardInput;

        return GetGamepadInput();
    }

    private void Update()
    {
        if (!canMove)
            return;

        HandleKeyboardAndGamepadActions();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Movement is now read directly from PlayerInput.
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Currently unused.
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        // Kick is now read directly from PlayerInput.
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // Jump is now read directly from PlayerInput.
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        // Sprint is now read directly from PlayerInput.
    }

    private void HandleKeyboardAndGamepadActions()
    {
        bool keyboardSprint = false;
        bool gamepadSprint = false;

        // -------------------------
        // KEYBOARD
        // -------------------------
        if (Keyboard.current != null)
        {
            if (playerNumber == 1)
            {
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    Kick();

                if (Keyboard.current.cKey.wasPressedThisFrame)
                    Jump();

                keyboardSprint = Keyboard.current.qKey.isPressed;
            }
            else if (playerNumber == 2)
            {
                if (Keyboard.current.semicolonKey.wasPressedThisFrame)
                    Kick();

                if (Keyboard.current.periodKey.wasPressedThisFrame)
                    Jump();

                keyboardSprint = Keyboard.current.uKey.isPressed;
            }
        }

        // -------------------------
        // GAMEPAD
        // -------------------------
        if (playerInput != null && playerInput.actions.enabled)
        {
            if (playerInput.actions["Kick"].WasPressedThisFrame())
            {
                Kick();
            }

            if (playerInput.actions["Jump"].WasPressedThisFrame())
            {
                Jump();
            }

            gamepadSprint =
                playerInput.actions["Sprint"].IsPressed();
        }

        // Sprint if EITHER input is being held
        _isRunning = keyboardSprint || gamepadSprint;
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void Jump()
    {
        if (!canMove)
            return;

        if (!IsGrounded())
            return;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            jumpForce,
            rb.linearVelocity.z
        );
    }

    // =========================================================
    // KICK
    // =========================================================

    private void Kick()
    {
        if (!canMove)
            return;

        Collider[] hits = Physics.OverlapSphere(
            RayPoint.position,
            kickRadius,
            FootBallLayer
        );

        if (hits.Length > 0)
        {
            Rigidbody ballRb = hits[0].attachedRigidbody;

            if (ballRb != null)
            {
                Quaternion tiltRotation =
                    Quaternion.AngleAxis(
                        -upAngle,
                        transform.right
                    );

                Vector3 finalDirection =
                    tiltRotation * RayPoint.forward;

                ballRb.linearVelocity = Vector3.zero;

                ballRb.AddForce(
                    finalDirection * _kickForce,
                    ForceMode.Impulse
                );
            }
        }

        if (animationManager != null &&
            !animationManager.IsExploding)
        {
            animationManager.PlayShoot();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & FootBallLayer) == 0)
            return;

        Rigidbody ballRb = collision.rigidbody;

        if (ballRb == null)
            return;

        Vector3 pushDirection = collision.transform.position - transform.position;
        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude <= 0.001f)
            return;

        pushDirection.Normalize();

        float force = _isRunning ? ballPushForce * SpeedMultiplier : ballPushForce;

        ballRb.AddForce(
            pushDirection * force,
            ForceMode.Impulse
        );
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );

            return;
        }

        Vector2 movementInput = GetMovementInput();

        Vector3 inputDir = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        // Don't move while exploding.
        if (animationManager != null &&
            animationManager.IsExploding)
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );

            return;
        }

        float currentSpeed = _isRunning
            ? speed * SpeedMultiplier
            : speed;

        Vector3 desiredMovement =
    inputDir * currentSpeed * Time.fixedDeltaTime;

        Vector3 safeMovement =
            GetSafeMovement(desiredMovement);

        rb.MovePosition(
            rb.position + safeMovement
        );

        HandleMovementDust(inputDir);

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(inputDir);

            transform.rotation = targetRotation;
        }

        UpdateAnimationState(inputDir);

        DisplayPlayerPosition();
    }

    private Vector3 GetSafeMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return Vector3.zero;

        Vector3 direction = movement.normalized;
        float distance = movement.magnitude;

        Collider playerCollider = GetComponent<Collider>();

        if (playerCollider == null)
            return movement;

        Bounds bounds = playerCollider.bounds;

        Vector3 point1 = bounds.center + Vector3.up * (bounds.extents.y * 0.5f);
        Vector3 point2 = bounds.center - Vector3.up * (bounds.extents.y * 0.5f);

        float radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
        int movementMask =
            Physics.DefaultRaycastLayers & ~FootBallLayer.value;

        if (Physics.CapsuleCast(
            point1,
            point2,
            radius,
            direction,
            out RaycastHit hit,
            distance,
            movementMask,
            QueryTriggerInteraction.Ignore))
        {
            // Remove the part of the movement going INTO the wall.
            Vector3 slideMovement =
                Vector3.ProjectOnPlane(movement, hit.normal);

            return slideMovement;
        }

        return movement;
    }

    // =========================================================
    // DUST
    // =========================================================

    private void HandleMovementDust(Vector3 inputDir)
    {
        if (DustPool.Instance == null)
            return;

        if (!IsGrounded())
        {
            nextDustTime = 0f;
            return;
        }

        if (inputDir.sqrMagnitude <= dustMoveThreshold)
        {
            nextDustTime = 0f;
            return;
        }

        if (Time.time >= nextDustTime)
        {
            Vector3 movementDirection = inputDir.normalized;

            Vector3 spawnPosition =
                transform.position
                - movementDirection * dustSpawnBehindDistance
                + Vector3.up * dustSpawnHeight;

            DustPool.Instance.Spawn(spawnPosition);

            nextDustTime =
                Time.time + dustInterval;
        }
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void UpdateAnimationState(Vector3 inputDir)
    {
        if (animationManager == null)
            return;

        if (animationManager.IsExploding)
            return;

        bool isMoving =
            inputDir.sqrMagnitude > 0.001f;

        if (!isMoving)
        {
            animationManager.PlayIdle();
        }
        else if (_isRunning)
        {
            animationManager.PlaySprint();
        }
        else
        {
            animationManager.PlayRun();
        }
    }

    // =========================================================
    // POSITION INDICATORS
    // =========================================================

    private void DisplayPlayerPosition()
    {
        if (playerPositionIndicator == null ||
            playerAimIndicator == null)
        {
            return;
        }

        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            -transform.up,
            out hit,
            RayDistance))
        {
            Vector3 floorPosition =
                new Vector3(
                    hit.point.x,
                    hit.point.y + _floorOffset,
                    hit.point.z
                );

            playerPositionIndicator.position =
                floorPosition;

            playerAimIndicator.position =
                floorPosition;
        }
    }

    // =========================================================
    // GROUNDED
    // =========================================================

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            2.2f
        );
    }

    // =========================================================
    // MOVEMENT LOCK
    // =========================================================

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            _isRunning = false;

            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (RayPoint == null)
            return;

        bool wouldHit =
            Physics.CheckSphere(
                RayPoint.position,
                kickRadius,
                FootBallLayer
            );

        Gizmos.color =
            wouldHit
                ? Color.red
                : Color.green;

        Gizmos.DrawWireSphere(
            RayPoint.position,
            kickRadius
        );

        Gizmos.color =
            wouldHit
                ? new Color(1f, 0f, 0f, 0.15f)
                : new Color(0f, 1f, 0f, 0.15f);

        Gizmos.DrawSphere(
            RayPoint.position,
            kickRadius
        );
    }
}