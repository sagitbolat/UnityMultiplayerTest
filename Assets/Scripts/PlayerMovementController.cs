using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundRadius = 0.4f;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private CharacterController controller = null;

    private Vector2 previousInput;
    private Vector3 velocity;
    private bool isGrounded;

    private PlayerControls controls;
    private PlayerControls Controls {
        get {
            if (controls != null) { return controls; }
            return controls = new PlayerControls();
        }
    }

    public override void OnStartAuthority() {
        enabled = true;

        Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        Controls.Player.Move.canceled += ctx => ResetMovement();
        Controls.Player.Jump.performed += ctx => Jump();

        Cursor.lockState = CursorLockMode.Locked;
    }

    [ClientCallback]
    private void Update() => Move();

    [ClientCallback]
    private void OnEnable() => Controls.Enable();

    [ClientCallback]
    private void OnDisable() => Controls.Disable();

    [Client]
    private void SetMovement(Vector2 movement) => previousInput = movement;

    [Client]
    private void ResetMovement() => previousInput = Vector2.zero;

    private void Jump() {
        if (!isGrounded) return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        controller.Move(velocity * Time.deltaTime);
    }

    [Client]
    private void Move() {
        //WASD movement
        Vector3 right = controller.transform.right;
        Vector3 forward = controller.transform.forward;
        right.y = 0f;
        forward.y = 0f;

        Vector3 movement = right.normalized * previousInput.x + forward.normalized * previousInput.y;

        controller.Move(movement * movementSpeed * Time.deltaTime);

        //Gravity and jumping
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
