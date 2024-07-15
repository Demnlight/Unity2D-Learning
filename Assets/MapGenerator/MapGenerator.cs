using System;
using Scripts.AdditionalMath;
using Scripts.Chunks;
using Scripts.MapGenerator;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
    private ChunkManager chunkManager;
    private SceneObjectsGenerator objectsGenerator = new SceneObjectsGenerator( );
    private Vector2Int vLastPosition = Vector2Int.zero;

    [SerializeField] private Transform pPlayerTransform;
    [SerializeField] private Material[ ] aMaterials;
    [SerializeField] private TileBase[ ] pTiles = null;

    private Tilemap[ ] pMaps = null;

    [SerializeField] public float[ ] pMinTilesHeights = null;
    [SerializeField] public float[ ] pMaxTilesHeights = null;

    public void Init( ) {
        if (pTiles == null)
            throw new InvalidOperationException( "pTiles null" );

        if (objectsGenerator == null)
            throw new InvalidOperationException( "Objects Generator null" );

        const int nTileMapsCount = 3;
        Transform parent = GameObject.FindGameObjectWithTag( "MainGrid" ).transform;
        TilemapGenerator_t tilemapGenerator = new TilemapGenerator_t {
            nTileMapsCount = nTileMapsCount,
            aTileMapsNames = new string[ nTileMapsCount ] { "WaterShadowMap", "WaterMap", "SandMap" },
            aParents = new Transform[ nTileMapsCount ] { parent, parent, parent },
            aColors = new Color[ nTileMapsCount ] { new Color( 0, 0, 137f / 255.0f, 25.0f / 255.0f ), Color.white, Color.white },
            aPositions = new Vector3[ nTileMapsCount ] { new Vector3( 0, 0, 0.1f ), new Vector3( 0, 0, 0.0f ), new Vector3( 0, 0, 0.9f ) },
            aMaterials = aMaterials,
        };

        pMaps = objectsGenerator.GenerateTileMaps( tilemapGenerator );
        if (pMaps == null)
            throw new InvalidOperationException( "pMaps null" );

        if (pMinTilesHeights == null)
            throw new InvalidOperationException( "pMinTilesHeights null" );

        if (pMaxTilesHeights == null)
            throw new InvalidOperationException( "pMaxTilesHeights null" );

        if (aMaterials == null)
            throw new InvalidOperationException( "aMaterials null" );

        chunkManager = new ChunkManager( pTiles, pMaps, pMinTilesHeights, pMaxTilesHeights, aMaterials );
        chunkManager.ClearChunks( );

        chunkManager.GenerateChunksAround( AdditionalMath.GetCurrentChunkWorldPos( pPlayerTransform.position ), ChunkConstants.nRenderDistance );
    }

    void Update( ) {
        Vector2Int vLocalPosScaled = AdditionalMath.GetCurrentChunkScaledPos( pPlayerTransform.position );
        if (vLastPosition != vLocalPosScaled) {
            Vector2Int vLocalPos = AdditionalMath.GetCurrentChunkWorldPos( pPlayerTransform.position );

            chunkManager.GenerateChunksAround( vLocalPos, ChunkConstants.nRenderDistance );

            vLastPosition = vLocalPosScaled;
        }
    }

    private void OnDrawGizmos( ) {
        if (chunkManager == null)
            return;

        /*foreach (var c in chunkManager.GetRenderingChunks( )) {
            Vector3 ChunkPosition = new Vector3( c.Value.GetPosition.x * ChunkConstants.nChunkSize, c.Value.GetPosition.y * ChunkConstants.nChunkSize );
            for (int x = 0; x < ChunkConstants.nChunkSize; x++) {
                for (int y = 0; y < ChunkConstants.nChunkSize; y++) {
                    Vector3 vCellPos = new Vector3( c.Value.GetPosition.x + x + 0.5f, c.Value.GetPosition.y + y + 0.5f, 0 );

                    float flHeight = c.Value.GetHeights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }*/
    }

    public IChunkManager GetChunkManager( ) => this.chunkManager;

    public TileBase GetTileBase { get { return this.pTiles[ this.pTiles.Length - 1 ]; } }
    public Tilemap GetTileMap { get { return this.pMaps[ this.pMaps.Length - 1 ]; } }
    public Material GetMaterial { get { return this.aMaterials[ this.aMaterials.Length - 1 ]; } }
}