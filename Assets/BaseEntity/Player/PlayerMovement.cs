using System;
using System.Collections.Generic;
using Scripts.MapGenerator;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent( typeof( Rigidbody2D ) )]
public class playerMovement : MonoBehaviour {
    [SerializeField, Range( 0, 9999 )] private float maxSpeed = 250f;
    [SerializeField, Range( 0, 0.99f )] private float friction = 0.9f;

    [SerializeField] private Generator mapGenerator = null;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Vector2 vMovingDirection;

    private Transform pSkeletonTransform = null;
    private bool bSwimming = false;
    public void Init( ) {
        rb = GetComponent<Rigidbody2D>( );
        pSkeletonTransform = this.gameObject.transform.GetChild( 0 ).transform;

        inputActions = new PlayerInputActions( );
        inputActions.Player.Enable( );
    }

    private void FixedUpdate( ) {
        if (inputActions == null)
            throw new ArgumentException( "inputActions == null" );

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

    private void OnTriggerEnter2D( Collider2D other ) {
        if (other.gameObject.CompareTag( "Water" )) {
            this.bSwimming = true;
        }
    }
    private void OnTriggerStay2D( Collider2D other ) {
        if (pSkeletonTransform != null) {
            if (this.bSwimming) {
                Vector2Int vChunkPos = new Vector2Int( );
                vChunkPos.x = mapGenerator.pHelper.Rounded( this.transform.position.x / Scripts.MapGenerator.ChunkConsts.nChunkSize ) * Scripts.MapGenerator.ChunkConsts.nChunkSize;
                vChunkPos.y = mapGenerator.pHelper.Rounded( this.transform.position.y / Scripts.MapGenerator.ChunkConsts.nChunkSize ) * Scripts.MapGenerator.ChunkConsts.nChunkSize;
                Chunk_t pCurrentChunk = mapGenerator.pChunksData.GetChunk( vChunkPos.x, vChunkPos.y );

                Vector2Int vPlayerCoordsInChunk = new Vector2Int(
                    Math.Abs( pCurrentChunk.vPos.x - mapGenerator.pHelper.Rounded( this.transform.position.x ) ),
                    Math.Abs( pCurrentChunk.vPos.y - mapGenerator.pHelper.Rounded( this.transform.position.y ) )
                );

                float flCurrentHeight = pCurrentChunk.Heights[ vPlayerCoordsInChunk.x, vPlayerCoordsInChunk.y ];

                //Debug.LogFormat( "[{0}, {1}], H: {2}, ", vPlayerCoordsInChunk.x, vPlayerCoordsInChunk.y, flCurrentHeight );
                flCurrentHeight = Mathf.InverseLerp( 0.50f, 0, flCurrentHeight ) * 2.5f;
                //Debug.LogFormat( "SH: {0}", flCurrentHeight );

                Vector3 vTargetPos = new Vector3( 0, -flCurrentHeight, 0 );

                float flDistance2D = Vector3.Distance( pSkeletonTransform.localPosition, vTargetPos );
                float flProgress = Math.Max( 1.0f, flDistance2D );

                pSkeletonTransform.localPosition = Vector3.LerpUnclamped( pSkeletonTransform.localPosition, vTargetPos, flProgress * 3 * Time.deltaTime );
            }
        }
    }
    private void OnTriggerExit2D( Collider2D other ) {
        if (other.gameObject.CompareTag( "Water" )) {
            this.bSwimming = false;
        }
    }
}