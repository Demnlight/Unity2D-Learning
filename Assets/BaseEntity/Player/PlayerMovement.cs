using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour {
    [SerializeField] private float maxSpeed = 250f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float friction = 0.9f;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 Last_Direction;
    private Vector2 vMovingDirection;

    void Awake( ) {
        rb = GetComponent<Rigidbody2D>( );

        inputActions = new PlayerInputActions( );
        inputActions.Player.Enable( );
    }

    private void FixedUpdate( ) {
        vMovingDirection = inputActions.Player.Move.ReadValue<Vector2>( );
        /*if (vMovingDirection != Vector2.zero) {
            if (vMovingDirection != Last_Direction) {
                rigibody.velocity = Vector2.zero;
                Last_Direction = vMovingDirection;
            }

            rigibody.velocity += vMovingDirection * Accelaration * Time.deltaTime;

            if (rigibody.velocity.magnitude > Max_Speed) {
                rigibody.velocity = rigibody.velocity.normalized * Max_Speed;
            }
        } else {
            rigibody.velocity -= rigibody.velocity.normalized * Deccelaration * Time.deltaTime;

            if (rigibody.velocity.magnitude < 0.1f) {
                rigibody.velocity = Vector2.zero;
            }
        }

        if (vMovingDirection.x != 0)
            transform.localScale = new Vector3( vMovingDirection.x, transform.localScale.y, transform.localScale.z );*/

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