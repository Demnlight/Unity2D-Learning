using UnityEngine;

public class Initialization : MonoBehaviour {
    [SerializeField] private playerMovement playerMovement;
    [SerializeField] private FovManager fovManager;
    [SerializeField] private MapGenerator mapGenerator;

    private void Awake( ) {
        fovManager.Init( );
        playerMovement.Init( );
        mapGenerator.Init( );
    }
}