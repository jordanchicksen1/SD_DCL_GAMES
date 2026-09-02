using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
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
    public float SpeedMultiplier;

    public LayerMask FootBallLayer;
    [SerializeField]
    private Transform RayPoint;
    [SerializeField]
    private float RayDistance;
    public int _kickForce;
    [SerializeField] private float upAngle = 30f;
    [SerializeField] private Transform playerPositionIndicator, playerAimIndicator;
    [SerializeField]
    private float _floorOffset;
    [SerializeField]
    private float kickRadius = 0.5f;

    [Header("Animation")]
    public AnimationManager animationManager;

    private bool _isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.defaultActionMap = "UI";
        Cursor.lockState = CursorLockMode.None;
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed = speed * SpeedMultiplier;
            _isRunning = true;
        }
        else if (context.canceled)
        {
            speed = speed / SpeedMultiplier;
            _isRunning = false;
        }
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        Collider[] hits = Physics.OverlapSphere(RayPoint.position, kickRadius, FootBallLayer);

        if (hits.Length > 0)
        {
            Rigidbody ballRb = hits[0].attachedRigidbody;
            if (ballRb != null)
            {
                Quaternion tiltRotation = Quaternion.AngleAxis(-upAngle, transform.right);
                Vector3 finalDirection = tiltRotation * RayPoint.forward;

                ballRb.linearVelocity = Vector3.zero;
                ballRb.AddForce(finalDirection * _kickForce, ForceMode.Impulse);

                if (animationManager != null)
                    animationManager.PlayShoot();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (RayPoint == null) return;

        bool wouldHit = Physics.CheckSphere(RayPoint.position, kickRadius, FootBallLayer);

        Gizmos.color = wouldHit ? Color.red : Color.green;
        Gizmos.DrawWireSphere(RayPoint.position, kickRadius);

        Gizmos.color = wouldHit
            ? new Color(1f, 0f, 0f, 0.15f)
            : new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(RayPoint.position, kickRadius);
    }

    void DisplayPlayerPosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, RayDistance))
        {
            Vector3 floorPosition = new Vector3(hit.point.x, hit.point.y + _floorOffset, hit.point.z);
            playerPositionIndicator.position = floorPosition;
            playerAimIndicator.position = floorPosition;
        }
    }

    void FixedUpdate()
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.z);

        rb.MovePosition(rb.position + (inputDir * speed) * Time.fixedDeltaTime);

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = targetRotation;
        }

        UpdateAnimationState(inputDir);
        DisplayPlayerPosition();
    }

    private void UpdateAnimationState(Vector3 inputDir)
    {
        if (animationManager == null) return;

        bool isMoving = inputDir.sqrMagnitude > 0.001f;

        if (!isMoving)
            animationManager.PlayIdle();
        else if (_isRunning)
            animationManager.PlaySprint();
        else
            animationManager.PlayRun();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}