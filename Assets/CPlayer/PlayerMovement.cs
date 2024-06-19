using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    // Update is called once per frame
    static public void Update( BasePlayer pPlayer ) {

        float flAxisRawHorizontal = Input.GetAxisRaw( "Horizontal" );
        float flAxisRawVertical = Input.GetAxisRaw( "Vertical" );

        //Move Left
        //if ( AxisRawHorizontal < 0 ) {
        //    pPlayer.flSideMove = -1.0f;
        //} else if ( AxisRawHorizontal > 0 ) {
        //    pPlayer.flSideMove = 1.0f;
        //} else
        //    pPlayer.flSideMove = 0.0f;

        //if ( AxisRawVertical < 0 ) {
        //    pPlayer.flForwardMove = -1.0f;
        //} else if ( AxisRawVertical > 0 ) {
        //    pPlayer.flForwardMove = 1.0f;
        //} else
        //    pPlayer.flForwardMove = 0.0f;

        if ( !pPlayer.bIsSliding ) {
            pPlayer.flSideMove = flAxisRawHorizontal;
            pPlayer.flForwardMove = flAxisRawVertical;

            pPlayer.vMovingDirection = new Vector2( pPlayer.flSideMove, pPlayer.flForwardMove );
        }

        if ( pPlayer.vMovingDirection != pPlayer.vLastMovingDirection ) {
            pPlayer.vLastMovingDirection = pPlayer.vMovingDirection;
            pPlayer.vLastVelocity = pPlayer.vVelocity;
            pPlayer.flStopTime = pPlayer.flAcceleration / 250.0f;
            pPlayer.bIsSliding = true;
            pPlayer.bIsMoving = false;

            pPlayer.pAnimator.SetTrigger( "OnPlayerStop" );
        }

        if ( !pPlayer.bIsSliding ) {

            if ( pPlayer.flSideMove > 0.0f || pPlayer.flSideMove < 0.0f || pPlayer.flForwardMove > 0.0f || pPlayer.flForwardMove < 0.0f ) {
                pPlayer.bIsMoving = true;
                pPlayer.flTimeSinceStartMoving += 1.0f * Time.deltaTime;

                pPlayer.flAcceleration = Mathf.Lerp( pPlayer.flAcceleration, 250.0f, pPlayer.flTimeSinceStartMoving / 5 );

            } else {
                pPlayer.flAcceleration = Mathf.Lerp( pPlayer.flAcceleration, 0.0f, 1.0f - pPlayer.flStopTime );
                pPlayer.bIsMoving = false;
                pPlayer.flTimeSinceStartMoving = 0.0f;
            }

            pPlayer.pAnimator.SetFloat( "flRunningAnimationSpeed", pPlayer.flAcceleration / 250.0f );
        }

        if ( pPlayer.flStopTime > 0.0f ) {
            pPlayer.flStopTime -= 1.0f * Time.deltaTime;
            pPlayer.flStopTime = Mathf.Clamp( pPlayer.flStopTime, 0.0f, 1.0f );

            Vector2 vStopVelocity = pPlayer.vLastVelocity * 1000 * ( pPlayer.flAcceleration / 250.0f );

            pPlayer.pRigiBody.AddForce( vStopVelocity, ForceMode2D.Force );
            Debug.LogFormat( "AddForce to BasePlayer. [ {0}, {1} ]", vStopVelocity.x, vStopVelocity.y );

        } else {
            pPlayer.bIsSliding = false;
        }

        pPlayer.pAnimator.SetBool( "isRun", pPlayer.bIsMoving );
        pPlayer.pAnimator.SetBool( "isSliding", pPlayer.bIsSliding );

        pPlayer.flLastForwardMove = pPlayer.flForwardMove;
        pPlayer.flLastSideMove = pPlayer.flSideMove;
    }
}
