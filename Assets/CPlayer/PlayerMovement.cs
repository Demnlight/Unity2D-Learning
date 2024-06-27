using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.Experimental.GraphView.GraphView;

public class playerMovement : MonoBehaviour {
    [SerializeField] private float Accelaration_Player = 50f;
    [SerializeField] private float Deccelaration_Player = 50f;
    [SerializeField] private float Max_Speed = 250f;

    private Rigidbody2D rigibody;

    private void Awake( ) {
        rigibody = GetComponent<Rigidbody2D>( );
    }

    private void FixedUpdate( ) {
        float flAxisRawHorizontal = Input.GetAxisRaw( "Horizontal" );
        float flAxisRawVertical = Input.GetAxisRaw( "Vertical" );

        Vector2 inputVector = new Vector2( flAxisRawHorizontal, flAxisRawVertical );

        if (inputVector != Vector2.zero) {
            rigibody.AddForce( inputVector * Accelaration_Player, ForceMode2D.Force );

            if (rigibody.velocity.magnitude > Max_Speed) {
                rigibody.velocity = rigibody.velocity.normalized * Max_Speed;
            }
        Vector2 inputVector = new Vector2( flAxisRawHorizontal, flAxisRawVertical );

        if (inputVector != Vector2.zero) {
            rigibody.AddForce( inputVector * Accelaration_Player, ForceMode2D.Force );

            if (rigibody.velocity.magnitude > Max_Speed) {
                rigibody.velocity = rigibody.velocity.normalized * Max_Speed;
            }
        } else {
            rigibody.AddForce( rigibody.velocity * -Deccelaration_Player, ForceMode2D.Force );
        }
            rigibody.AddForce( rigibody.velocity * -Deccelaration_Player, ForceMode2D.Force );
        }
    }
}