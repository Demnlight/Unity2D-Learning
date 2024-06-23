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
    public static void CreatePlayer( ) {
        BasePlayer pNewPlayer = new BasePlayer( );
        m_aPlayers.Add( pNewPlayer );
    }
    public static void AddPlayer( BasePlayer pNewPlayer ) {
        m_aPlayers.Add( pNewPlayer );
    }

    void ControllPlayerMovement( ) {
        foreach ( BasePlayer pPlayer in m_aPlayers ) {

            PlayerMovement.CalculateMoveData( pPlayer );
            
            pPlayer.pRigiBody.velocity = pPlayer.vVelocity;
        }
    }
}