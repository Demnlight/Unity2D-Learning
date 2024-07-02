using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Scripts.MapGenerator;
using Scripts.Movement;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent( typeof( Rigidbody2D ) )]
public class PlayerMovement : MonoBehaviour {
    private PlayerInputActions inputActions;
    private Rigidbody2D rb = null;

    [SerializeField] private Animator pAnimator = null;
    [SerializeField] private Transform pSkeletonTransform = null;
    [SerializeField, Range( 0, 1000 )] private float flMaxSpeed = 250f;
    [SerializeField, Range( 0, 0.99f )] private float flFriction = 0.9f;
    [SerializeField] private Generator mapGenerator = null;

    //private bool bSwimming = false;
    private bool bInWater = false;

    private bool bWantBoost = false;

    private SpeedModifers nSpeedModifer = SpeedModifers.NORMAl;

    private Vector2 vMovingDirection = new Vector2( );

    Dictionary<SpeedModifers, IMovement> aMovements = new Dictionary<SpeedModifers, IMovement>( );

    delegate void Move( float flSpeedModifier );
    public void Init( ) {
        if (!pSkeletonTransform)
            throw new InvalidOperationException( "pSkeletonTransform null" );

        rb = GetComponent<Rigidbody2D>( );

        inputActions = new PlayerInputActions( );
        inputActions.Player.Enable( );

        aMovements = new Dictionary<SpeedModifers, IMovement> {
            {SpeedModifers.SWIMMING, new SwimmingMovement( ) },
            {SpeedModifers.SLOWED, new SlowedMovement( ) },
            {SpeedModifers.NORMAl, new WalkingMovement( ) },
            {SpeedModifers.BOOSTED, new RunningMovement( ) },
        };
    }

    private void FixedUpdate( ) {
        if (inputActions == null)
            throw new InvalidOperationException( "inputActions null" );

        this.vMovingDirection = inputActions.Player.Direction.ReadValue<Vector2>( ).normalized;
        if (this.vMovingDirection == Vector2.zero && rb.velocity.magnitude <= 0)
            return;

        if (this.nSpeedModifer != SpeedModifers.SWIMMING) {
            this.bWantBoost = inputActions.Player.Boost.ReadValue<float>( ) > 0;
            this.nSpeedModifer = this.bWantBoost ? SpeedModifers.BOOSTED : SpeedModifers.NORMAl;
        }

        MovementDTO movementDTO = new MovementDTO {
            flFriction = this.flFriction,
            flMaxSpeed = this.flMaxSpeed,
            rb = this.rb,
            vDirection = this.vMovingDirection
        };

        aMovements[ this.nSpeedModifer ].Move( movementDTO );

        this.pAnimator.SetBool( "IsSwimming", this.nSpeedModifer == SpeedModifers.SWIMMING );
        this.pAnimator.SetBool( "IsRunning", this.nSpeedModifer == SpeedModifers.BOOSTED );
        this.pAnimator.SetFloat( "flRunningAnimationSpeed", rb.velocity.magnitude / this.flMaxSpeed );

        if (this.vMovingDirection.x != 0)
            this.transform.localScale = new Vector3( this.vMovingDirection.x > 0 ? 1 : -1, 1, 1 );
    }

    private void OnTriggerEnter2D( Collider2D other ) {
        if (other.gameObject.CompareTag( "Water" )) {
            this.bInWater = true;
        }
    }

    private void OnTriggerStay2D( Collider2D other ) {
        if (pSkeletonTransform != null) {
            if (this.bInWater) {
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

                pSkeletonTransform.localPosition = Vector3.LerpUnclamped( pSkeletonTransform.localPosition, vTargetPos, flProgress * 5 * Time.deltaTime );

                this.nSpeedModifer = flCurrentHeight >= 1.75f ? SpeedModifers.SWIMMING : SpeedModifers.NORMAl;
            }
        }
    }

    private void OnTriggerExit2D( Collider2D other ) {
        if (other.gameObject.CompareTag( "Water" )) {
            this.bInWater = false;

            pSkeletonTransform.localPosition = new Vector3( 0, 0, 0 );
        }
    }
}