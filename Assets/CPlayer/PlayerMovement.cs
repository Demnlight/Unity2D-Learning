using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class playerMovement : MonoBehaviour {
    [SerializeField] private float Accelaration_Player = 5f;
    [SerializeField] private float Deccelaration_Player = 5f;
    [SerializeField] private float Max_Speed = 20f;

    private Rigidbody2D rigibody;

    private void Awake( ) {
        rigibody = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        } 
        else {
            rigibody.AddForce( rigibody.velocity * -Deccelaration_Player, ForceMode2D.Force );
        }
    }
}
