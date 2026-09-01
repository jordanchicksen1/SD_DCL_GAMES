using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private float RunSpeed;

    private GameObject InteractableObject;
    public LayerMask Interact;
    [SerializeField]
    private Transform RayPoint;


    //Player Assortment Manager
    [SerializeField]
    private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject PauseFirstSelect, InventoryFirstSelect;

    [Header("Knockback")]
    [SerializeField]
    private float knockbackDrag = 5f; // higher = knockback fades out faster
    private Vector3 knockbackVelocity;
    [SerializeField]
    private float throwForce;

  
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Called by external systems (e.g. BombManager) to push the player.
    // Needed because normal movement uses MovePosition every FixedUpdate,
    // which would otherwise instantly cancel out any physics force applied
    // directly to the Rigidbody (like AddExplosionForce).
    public void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity += force;
    }

    void Start()
    {
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.defaultActionMap = "UI";
        Cursor.lockState = CursorLockMode.None;

        RunSpeed = speed * SpeedMultiplier;

        playerInput = GetComponent<PlayerInput>();
    }


    // MOVEMENT
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }

    //Inventory System

    // LOOK
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }


    // Pause/Play

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
        }
        else if (context.canceled)
        {
            speed = speed / SpeedMultiplier;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {

    }

    public void OnGameSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
            SceneManager.LoadScene("GameSelect");
    }



    void FixedUpdate()
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.z);

        // Decay any active knockback so it fades out rather than persisting forever
        if (knockbackVelocity.sqrMagnitude > 0.01f)
        {
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDrag * Time.fixedDeltaTime);
        }
        else
        {
            knockbackVelocity = Vector3.zero;
        }

        // Move relative to world (NOT current rotation), blended with any knockback
        rb.MovePosition(rb.position + (inputDir * speed + knockbackVelocity) * Time.fixedDeltaTime);

        // Rotate ONLY when moving
        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.fixedDeltaTime
            );
        }

    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}