using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

using Scripts.MapGenerator;

public class Generator : MonoBehaviour {
    [SerializeField, Range( 0, 9999.0f )] private float flPerlinScale = 48.0f;
    [SerializeField, Range( 0, 9999 )] private int flPerlinOctaves = 5;
    [SerializeField, Range( 0, 10 )] private float flPersistence = 1f;
    [SerializeField, Range( 0, 10 )] private float flLacunarity = 1f;
    [SerializeField, Range( 0, 10 )] private float flPerlinBaseAmplitude = 0.50f;
    [SerializeField, Range( 0, 320000 )] private int nSeed = 16275;

    [SerializeField] private TileBase[ ] pTiles = null;
    [SerializeField] public Tilemap[ ] pMaps = null;
    [SerializeField] public float[ ] pMinTilesHeights = null;
    [SerializeField] public float[ ] pMaxTilesHeights = null;
    [SerializeField] private Material pWaterMaterial = null;
    [SerializeField] private Material pSandMaterial = null;

    [SerializeField] private Transform pWizardTransform = null;

    private Texture2D pHeightMapTexture = null;

    private Vector2Int vStartChunk = Vector2Int.zero;
    private Vector2Int vPlayerChunkStart = Vector2Int.zero;

    public Helper pHelper = new Helper( );
    public ChunksData pChunksData = new ChunksData( );

    public void Init( ) {
        pChunksData.aAllChunks.Clear( );
        pChunksData.aVisibleChunks.Clear( );

        pChunksData.pSettings = new PerlinGeneratorSettings {
            flPerlinScale = this.flPerlinScale,
            nPerlinOctaves = this.flPerlinOctaves,
            flPersistence = this.flPersistence,
            flLacunarity = this.flLacunarity,
            flPerlinBaseAmplitude = this.flPerlinBaseAmplitude
        };

        System.Random random = new System.Random( nSeed );
        pChunksData.pSettings.vRandomOffsets = new Vector2[ this.flPerlinOctaves ];
        for (int i = 0; i < this.flPerlinOctaves; i++)
            pChunksData.pSettings.vRandomOffsets[ i ] = new Vector2( random.Next( -10000, 10000 ), random.Next( -10000, 10000 ) );
    }

    private void Update( ) {
        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = pHelper.Rounded( this.pWizardTransform.transform.position.x / ChunkConsts.nChunkSize );
        vChunkPos.y = pHelper.Rounded( this.pWizardTransform.transform.position.y / ChunkConsts.nChunkSize );
        UpdateChunks( vChunkPos );
    }

    private void UpdateChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkConsts.nChunkSize, vChunkPos.y * ChunkConsts.nChunkSize );
        if (!pChunksData.PositionIsNew( vNewChunkPos ))
            return;

        vPlayerChunkStart = vNewChunkPos;

        pChunksData.GetNearestChunks( ChunkConsts.nRenderDistance );

        List<Chunk_t> vAddedChunks = pChunksData.GenerateVisibleChunks( vNewChunkPos );

        List<Vector2Int> vRemovedChunksCoord = this.ClearFarChunks( pChunksData.aVisibleChunks, ChunkConsts.nRenderDistanceSize );
        this.ClearFarChunks( pChunksData.aAllChunks, ChunkConsts.nRenderDistanceSize * 2 );

        vStartChunk = pHelper.FindStartChunk( pChunksData.aVisibleChunks );

        this.pHeightMapTexture = pHelper.GenerateHeightMapTexture( pChunksData.aVisibleChunks, this.vStartChunk );
        if (this.pHeightMapTexture != null) {
            this.pHeightMapTexture.Apply( );

            pHelper.SetupMaterialData( pWaterMaterial, pChunksData.aVisibleChunks, this.pHeightMapTexture, this.vStartChunk );
            pHelper.SetupMaterialData( pSandMaterial, pChunksData.aVisibleChunks, this.pHeightMapTexture, this.vStartChunk );

        }

        this.ClearTileMaps( vRemovedChunksCoord );
        this.FillTiles( vAddedChunks );

        pChunksData.vLastChunkPos = vNewChunkPos;
    }

    void FillTiles( List<Chunk_t> pAddedCoords ) {
        if (this.pMaps == null || this.pMaps.Length <= 0)
            return;

        foreach (Chunk_t pChunk in pAddedCoords) {
            TileBase[ ][ ] pTiles = new TileBase[ this.pMaps.Length ][ ];
            for (int nMapLayer = 0; nMapLayer < this.pMaps.Length; nMapLayer++) {
                pTiles[ nMapLayer ] = new TileBase[ ChunkConsts.nChunkSize * ChunkConsts.nChunkSize ];
                pTiles[ nMapLayer ] = pHelper.GetTiles( pChunk,
                    this.pMinTilesHeights[ nMapLayer ], this.pMaxTilesHeights[ nMapLayer ],
                    this.pTiles[ nMapLayer ]
                );

                BoundsInt bounds = new BoundsInt(
                    pChunk.vPos.x, pChunk.vPos.y, 0,
                    ChunkConsts.nChunkSize, ChunkConsts.nChunkSize, 1
                );

                this.pMaps[ nMapLayer ].SetTilesBlock( bounds, pTiles[ nMapLayer ] );
            }
        }
    }

    private void OnDrawGizmos( ) {
        /*foreach (var c in pChunksData.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.Value.vPos.x * ChunkConsts.nChunkSize, c.Value.vPos.y * ChunkConsts.nChunkSize );
            for (int x = 0; x < ChunkConsts.nChunkSize; x++) {
                for (int y = 0; y < ChunkConsts.nChunkSize; y++) {
                    Vector3 vCellPos = new Vector3( c.Value.vPos.x + x + 0.5f, c.Value.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Value.Heights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }*/
    }

    private void OnGUI( ) {
        /*Vector3 vMousePos = Input.mousePosition;
        Vector3 vWorldPosition = Camera.main.ScreenToWorldPoint( vMousePos );

        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = pHelper.Rounded( vWorldPosition.x / ChunkConsts.nChunkSize );
        vChunkPos.y = pHelper.Rounded( vWorldPosition.y / ChunkConsts.nChunkSize );

        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkConsts.nChunkSize, vChunkPos.y * ChunkConsts.nChunkSize );
        Chunk_t pChunk = pChunksData.GetChunk( vNewChunkPos.x, vNewChunkPos.y );

        if (pChunk.Heights != null) {
            int nX = Math.Abs( pChunk.vPos.x - (int)vWorldPosition.x );
            int nY = Math.Abs( pChunk.vPos.y - (int)vWorldPosition.y );
            Debug.LogFormat( "[{0}], [{1}]", nX, nY );
            float flHeight = pChunk.Heights[ nX, nY ];
            string szHeight = "Height: " + flHeight.ToString( );
            Vector3 vWorldChunkPos = new Vector3( vWorldPosition.x, vWorldPosition.y, 0 );
            Vector3 vWorldScreenPosition = Camera.main.WorldToScreenPoint( vWorldChunkPos );

            GUI.Label( new Rect( vWorldScreenPosition.x - 80, vWorldScreenPosition.y - 45, 160, 30 ), szHeight );
        }*/
    }

    private void ClearTileMaps( List<Vector2Int> vRemovedChunksCoord ) {
        foreach (var v in vRemovedChunksCoord) {
            BoundsInt bounds = new BoundsInt(
                v.x, v.y, 0,
                ChunkConsts.nChunkSize, ChunkConsts.nChunkSize, 1
            );
            TileBase[ ] nullTiles = new TileBase[ ChunkConsts.nChunkSize * ChunkConsts.nChunkSize ];
            for (int i = 0; i < this.pMaps.Length; i++)
                this.pMaps[ i ].SetTilesBlock( bounds, nullTiles );
        }
    }

    private bool IsChunkFar( Vector2Int vChunkPos, int nMaxDistance ) {
        int nXDistance = Math.Abs( vChunkPos.x - this.vPlayerChunkStart.x );
        int nYDistance = Math.Abs( vChunkPos.y - this.vPlayerChunkStart.y );
        bool bXOutOfDistance = nXDistance > nMaxDistance;
        bool bYOutOfDistance = nYDistance > nMaxDistance;

        return bXOutOfDistance || bYOutOfDistance;
    }

    private List<Vector2Int> ClearFarChunks( Dictionary<Vector2Int, Chunk_t> pChunks, int nMaxDistance ) {
        List<Vector2Int> vToRemoveCoords = new List<Vector2Int>( );
        foreach (var pChunk in pChunks) {
            if (this.IsChunkFar( pChunk.Value.vPos, nMaxDistance ))
                vToRemoveCoords.Add( pChunk.Value.vPos );
        }

        foreach (Vector2Int vCoords in vToRemoveCoords) {
            pChunks.Remove( vCoords );
        }

        return vToRemoveCoords;
    }
}
