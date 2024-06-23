using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayer : MonoBehaviour {
    public enum ePlayerState {
        IDLE = 0,
        MOVING,
        GLIDE
    }
    public Rigidbody2D pRigiBody;
    public Animator pAnimator;

    public float flMaxSpeed = 250.0f;
    public float flSpeedMulti = 1.0f;
    public float flSideMove = 0.0f;
    public float flForwardMove = 0.0f;
    public float flLastSideMove = 0.0f;
    public float flLastForwardMove = 0.0f;
    public float flTimeSinceStartMoving = 0.0f;
    public float flAcceleration = 1.0f;
    public ePlayerState nPlayerState = ePlayerState.IDLE;
    public bool bChangedDirection = false;
    public float flGlideTime = 0.0f;


    public bool bIsJumping = false;
    public float flTimeInAir = 0.0f;

    public Vector2 vVelocity = new Vector2( );
    public Vector2 vLastVelocity = new Vector2( );

    public Vector2 vMovingDirection = new Vector2( );
    public Vector2 vLastMovingDirection = new Vector2( );

    public bool bIsSliding = false;

    // Start is called before the first frame update
    void Start( ) {
        this.pRigiBody = GetComponent<Rigidbody2D>( );
        this.pAnimator = GetComponent<Animator>( );

        PlayerController.AddPlayer( this );
    }

    // Update is called once per frame
    void Update( ) {

    }

    public Vector2 CalcVelocity( float flSideMove = 0.0f, float flForwardMove = 0.0f ) {
        Vector2 flVelocity = new Vector2( 0, 0 );

        float flLocalSideMove = 0.0f;
        float flLocalForwardMove = 0.0f;

        if (flSideMove > 0.0f)
            flLocalSideMove = flSideMove;
        else
            flLocalSideMove = this.flSideMove;

        if (flForwardMove > 0.0f)
            flLocalForwardMove = flForwardMove;
        else
            flLocalForwardMove = this.flForwardMove;


        flVelocity.x += flLocalSideMove * this.flAcceleration * flSpeedMulti;
        flVelocity.y += flLocalForwardMove * this.flAcceleration * flSpeedMulti;

        flVelocity.x = Mathf.Clamp( flVelocity.x, -250.0f, 250.0f );
        flVelocity.y = Mathf.Clamp( flVelocity.y, -250.0f, 250.0f );

        flVelocity.x *= Time.deltaTime;
        flVelocity.y *= Time.deltaTime;


        return flVelocity;
    }
}