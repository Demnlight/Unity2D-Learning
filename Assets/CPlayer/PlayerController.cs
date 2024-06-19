using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    static public List<BasePlayer> m_aPlayers = new List<BasePlayer>( );

    // Start is called before the first frame update
    void Start( ) {

    }

    // Update is called once per frame
    void FixedUpdate( ) {
        ControllPlayerMovement( );
    }

    public static void CreatePlayer( string szPlayerName ) {
        BasePlayer pNewPlayer = new BasePlayer( );
        pNewPlayer.nHealth = 100;
        pNewPlayer.szName = szPlayerName;
        m_aPlayers.Add( pNewPlayer );
    }
    public static void CreatePlayer( ) {
        BasePlayer pNewPlayer = new BasePlayer( );
        pNewPlayer.nHealth = 100;
        pNewPlayer.szName = "Bot";
        m_aPlayers.Add( pNewPlayer );
    }
    public static void AddPlayer( BasePlayer pNewPlayer ) {
        m_aPlayers.Add( pNewPlayer );
    }

    void ControllPlayerMovement( ) {
        foreach ( BasePlayer pPlayer in m_aPlayers ) {

            PlayerMovement.Update( pPlayer );

            pPlayer.vVelocity = pPlayer.CalcVelocity( );

            //pPlayer.pRigiBody.AddForce( pPlayer.vVelocity, ForceMode2D.Force );

            pPlayer.pRigiBody.velocity = pPlayer.vVelocity;

            //float flNewPosX = pPlayer.transform.position.x + pPlayer.vVelocity.x * pPlayer.flTimeSinceStartMoving + ( pPlayer.flAcceleration * ( pPlayer.flTimeSinceStartMoving * pPlayer.flTimeSinceStartMoving ) ) / 2;
            //float flNewPosY = pPlayer.transform.position.y + pPlayer.vVelocity.y * pPlayer.flTimeSinceStartMoving + ( pPlayer.flAcceleration * ( pPlayer.flTimeSinceStartMoving * pPlayer.flTimeSinceStartMoving ) ) / 2;

            //pPlayer.transform.position = new Vector3( flNewPosX, flNewPosY, 0 );
        }
    }
}