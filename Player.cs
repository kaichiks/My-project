using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private GameInput gameInput;

    private bool isWalking;

    float playerSize = 0.7f;
    float playerHeight = 2f;

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 flatmoveDir = new Vector3(inputVector.x, 0.0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerSize, flatmoveDir,moveDistance );

        if (!canMove)
        {
            //Attempt to move only on x axis
            Vector3 flatmoveDirX = new Vector3(flatmoveDir.x, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerSize, flatmoveDirX, moveDistance);

            if(canMove)
            {
                flatmoveDir = flatmoveDirX; 
            }
            else
            {
                //Attempt to move only on z-axis
                Vector3 flatmoveDirZ = new Vector3(0,0,flatmoveDir.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerSize, flatmoveDirZ, moveDistance);

                if(canMove)
                {
                    flatmoveDir = flatmoveDirZ;
                }
                else
                {
                    //Do not move
                }
            }
        }

        if (canMove)
        {
            transform.position += flatmoveDir * moveDistance;
        }

        isWalking = flatmoveDir != Vector3.zero;

        if (flatmoveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, flatmoveDir, rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}