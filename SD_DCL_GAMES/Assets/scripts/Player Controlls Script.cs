using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController3D : MonoBehaviour
{
    [SerializeField]
    private Vector3 moveInput;

    private Vector2 lookInput;

    private Rigidbody rb;
    private PlayerInput playerInput;

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

    [SerializeField]
    private Transform RayPoint;

    [SerializeField]
    private float RayDistance = 1.1f;

    public int _kickForce = 10;

    [SerializeField]
    private float upAngle = 30f;

    [SerializeField]
    private Transform playerPositionIndicator;

    [SerializeField]
    private Transform playerAimIndicator;

    [SerializeField]
    private float _floorOffset;

    [SerializeField]
    private float kickRadius = 0.5f;

    [Header("Animation")]
    public AnimationManager animationManager;

    private bool _isRunning;

    private RespawnManager _spawnManager;
    private bool canMove = false;
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

        _spawnManager = FindFirstObjectByType<RespawnManager>();

        if (_spawnManager != null)
        {
            _spawnManager.AddPlayer(transform);
        }
        else
        {
            Debug.LogWarning(
                "PlayerController3D: No RespawnManager found in the scene."
            );
        }

        if (animationManager != null)
        {
            animationManager.PlayIdle();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {


        Vector2 input = context.ReadValue<Vector2>();

        moveInput = new Vector3(
            input.x,
            0f,
            input.y
        );
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canMove)
            return;


        if (context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                jumpForce,
                rb.linearVelocity.z
            );
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (!canMove)
            return;


        if (context.performed)
        {
            speed *= SpeedMultiplier;
            _isRunning = true;
        }
        else if (context.canceled)
        {
            speed /= SpeedMultiplier;
            _isRunning = false;
        }
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

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

        Vector3 inputDir = new Vector3(
            moveInput.x,
            0f,
            moveInput.z
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

        rb.MovePosition(
            rb.position +
            (inputDir * speed) *
            Time.fixedDeltaTime
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

    private void HandleMovementDust(Vector3 inputDir)
    {
        if (DustPool.Instance == null)
        {
            Debug.LogWarning("PLAYER: DustPool.Instance is NULL!");
            return;
        }

        if (!IsGrounded())
        {
            Debug.Log("PLAYER: Not grounded, no dust.");
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

            Debug.Log("PLAYER: Spawning dust at " + spawnPosition);

            DustPool.Instance.Spawn(spawnPosition);

            nextDustTime = Time.time + dustInterval;
        }
    }
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

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            2.2f
        );
    }

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

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            // Clear any input that was being held.
            moveInput = Vector3.zero;
            lookInput = Vector2.zero;

            // Stop horizontal movement.
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }
}