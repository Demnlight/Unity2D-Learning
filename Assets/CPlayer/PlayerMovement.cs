using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    static public void CalculateMoveData( BasePlayer pPlayer ) {
        float flAxisRawHorizontal = Input.GetAxisRaw( "Horizontal" );
        float flAxisRawVertical = Input.GetAxisRaw( "Vertical" );

        pPlayer.flSideMove = flAxisRawHorizontal;
        pPlayer.flForwardMove = flAxisRawVertical;
        pPlayer.vMovingDirection = new Vector2( pPlayer.flSideMove, pPlayer.flForwardMove );

        if (pPlayer.vMovingDirection != pPlayer.vLastMovingDirection) {
            pPlayer.bChangedDirection = true;
            //if ( pPlayer.flAcceleration >= 10.0f )
            //    pPlayer.flGlideTime = 1.0f;
        } else
            pPlayer.bChangedDirection = false;

        if (pPlayer.vMovingDirection != Vector2.zero && pPlayer.flGlideTime <= 0.0f) {
            pPlayer.nPlayerState = BasePlayer.ePlayerState.MOVING;
            pPlayer.flAcceleration = Mathf.Lerp( pPlayer.flAcceleration, pPlayer.flMaxSpeed, pPlayer.flTimeSinceStartMoving / 3 );
            pPlayer.flTimeSinceStartMoving += 1.0f * Time.deltaTime;
        } else {
            pPlayer.nPlayerState = BasePlayer.ePlayerState.IDLE;
            pPlayer.flTimeSinceStartMoving = 0.0f;
            pPlayer.flAcceleration = 0;
        }

        pPlayer.vVelocity = pPlayer.CalcVelocity( );

        pPlayer.pAnimator.SetBool( "IsRunning", pPlayer.vVelocity != Vector2.zero );
        pPlayer.pAnimator.SetFloat( "flRunningAnimationSpeed", pPlayer.flAcceleration / pPlayer.flMaxSpeed );

        if (pPlayer.vMovingDirection.x != 0.0f)
            pPlayer.transform.localScale = new Vector3( pPlayer.vMovingDirection.x, 1, 1 );

        pPlayer.vLastMovingDirection = pPlayer.vMovingDirection;
        pPlayer.vLastVelocity = pPlayer.vVelocity;
        pPlayer.flLastForwardMove = pPlayer.flForwardMove;
        pPlayer.flLastSideMove = pPlayer.flSideMove;
    }
}
