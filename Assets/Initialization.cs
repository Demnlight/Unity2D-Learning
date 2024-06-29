using UnityEngine;

public class Initialization : MonoBehaviour {
    [SerializeField] private playerMovement playerMovement;
    [SerializeField] private FovManager fovManager;
    [SerializeField] private Generator generator;

    private void Awake( ) {
        fovManager.Init( );
        playerMovement.Init( );
        generator.Init( );
    }
}