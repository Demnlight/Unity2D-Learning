using Scripts.Perlin;
using UnityEngine;

public class Initialization : MonoBehaviour {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FovManager fovManager;
    [SerializeField] private MapGenerator generator;

    public static PerlinGenerator perlinGenerator = new PerlinGenerator( ); //Singleton

    [SerializeField, Range( 0, 9999.0f )] private float flPerlinScale = 128.0f;
    [SerializeField, Range( 0, 9999 )] private int nPerlinOctaves = 3;
    [SerializeField, Range( 0, 10 )] private float flPersistence = 0.5f;
    [SerializeField, Range( 0, 10 )] private float flLacunarity = 2f;
    [SerializeField, Range( 0, 10 )] private float flPerlinBaseAmplitude = 0.50f;
    [SerializeField, Range( 0, 320000 )] private int nSeed = 16275;

    private void Awake( ) {
        fovManager.Init( );
        playerMovement.Init( );

        PerlinGeneratorSettings Settings = new PerlinGeneratorSettings {
            flPerlinScale = this.flPerlinScale,
            nPerlinOctaves = this.nPerlinOctaves,
            flPersistence = this.flPersistence,
            flLacunarity = this.flLacunarity,
            flPerlinBaseAmplitude = this.flPerlinBaseAmplitude,
        };

        System.Random random = new System.Random( nSeed );
        Settings.vRandomOffsets = new Vector2[ this.nPerlinOctaves ];
        for (int i = 0; i < this.nPerlinOctaves; i++)
            Settings.vRandomOffsets[ i ] = new Vector2( random.Next( -10000, 10000 ), random.Next( -10000, 10000 ) );

        perlinGenerator.Settings = Settings;

        generator.Init( );
    }
}