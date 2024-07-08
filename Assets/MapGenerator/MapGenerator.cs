using System;
using Scripts.AdditionalMath;
using Scripts.Chunks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
    private ChunkManager chunkManager;
    private Vector2Int vLastPosition = Vector2Int.zero;

    [SerializeField] private Transform pPlayerTransform;
    [SerializeField] private Material[ ] aMaterials;
    [SerializeField] private TileBase[ ] pTiles = null;
    [SerializeField] public Tilemap[ ] pMaps = null;
    [SerializeField] public float[ ] pMinTilesHeights = null;
    [SerializeField] public float[ ] pMaxTilesHeights = null;

    public void Init( ) {
        if (pTiles == null)
            throw new InvalidOperationException( "pTiles null" );

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
}