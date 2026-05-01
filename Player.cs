using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private bool isRunning;
    private float verticalVelocity;

    private const float PLAYER_SIZE = 0.7f;
    private const float PLAYER_HEIGHT = 2f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f; 
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, 1f).normalized;

        float moveDistance = moveSpeed * Time.deltaTime;

        // Apply gravity to vertical movement
        Vector3 movement = moveDir * moveDistance;
        movement.y = verticalVelocity * Time.deltaTime;

        CollisionFlags flags = characterController.Move(movement);

        isRunning = characterController.isGrounded;

        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward,
                new Vector3(moveDir.x, 0, moveDir.z),
                rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsRunning() => isRunning;
}
