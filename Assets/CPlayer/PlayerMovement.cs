using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class playerMovement : MonoBehaviour {
    [SerializeField] private float Accelaration_Player = 5f;
    [SerializeField] private float Deccelaration_Player = 10f;
    [SerializeField] private float Max_Speed = 5f;

    private Rigidbody2D rigibody;
    private Vector2 Last_Direction;

    private void Awake( ) {
        rigibody = GetComponent<Rigidbody2D>( );
    }
    // Start is called before the first frame update
    void Start( ) {

    }

    // Update is called once per frame
    void Update( ) {

    }

    private void FixedUpdate( ) {
        float flAxisRawHorizontal = Input.GetAxisRaw( "Horizontal" );
        float flAxisRawVertical = Input.GetAxisRaw( "Vertical" );

        Vector2 inputVector = new Vector2( flAxisRawHorizontal, flAxisRawVertical );

        float t = Time.deltaTime; // время с момента прошедшего кадра

        // провекра на смену направления
        if (inputVector != Vector2.zero && inputVector != Last_Direction) {
            rigibody.velocity = Vector2.zero; // обнуление скорости при смене направления
            Last_Direction = inputVector; // обнова последнего направления
        }

        if (inputVector != Vector2.zero) {
            // ускорение с учетом времени кадра 
            rigibody.velocity += inputVector * Accelaration_Player * t;

            // ограничение макс скорости
            if (rigibody.velocity.magnitude > Max_Speed) {
                rigibody.velocity = rigibody.velocity.normalized * Max_Speed;
            }
        } else {
            // замедление с учетом времени кадра
            rigibody.velocity -= rigibody.velocity.normalized * Deccelaration_Player * t;

      
            if (rigibody.velocity.magnitude < 0.1f) { // скорость близится к 0, если остановка
                rigibody.velocity = Vector2.zero;
            }
        }
    }
}

