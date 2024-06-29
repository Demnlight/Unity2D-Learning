using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour {
    [SerializeField] private float maxSpeed = 250f;
    [SerializeField] private float acceleration = 250f;
    [SerializeField] private float friction = 0.9f;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 vMovingDirection;

    void Awake( ) {
        rb = GetComponent<Rigidbody2D>( );

        inputActions = new PlayerInputActions( );
        inputActions.Player.Enable( );
    }

    private void FixedUpdate( ) {
        vMovingDirection = inputActions.Player.Move.ReadValue<Vector2>( );

        Vector2 targetVelocity = vMovingDirection * maxSpeed;
        Vector2 speedDiff = targetVelocity - rb.velocity;
        Vector2 accelerationVector = speedDiff * Time.deltaTime;

        if (vMovingDirection.magnitude == 0) {
            rb.velocity = new Vector2( rb.velocity.x * friction, rb.velocity.y * friction );
        } else {
            rb.velocity = new Vector2(
                Mathf.Clamp( rb.velocity.x + accelerationVector.x, -maxSpeed, maxSpeed ),
                Mathf.Clamp( rb.velocity.y + accelerationVector.y, -maxSpeed, maxSpeed )
            );
        }
    }
}