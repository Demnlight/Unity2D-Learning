using UnityEngine;

public class Initialization : MonoBehaviour {
    [SerializeField] private playerMovement playerMovement;
    [SerializeField] private FovManager fovManager;

    private void Awake( ) {
        fovManager.Init( );
        playerMovement.Init( );
    }
}