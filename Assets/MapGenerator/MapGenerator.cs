using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;
/*
TODO:
1) clear this.allchunks if distance > 3 * nChunkSize.
*/
public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
}
public class MapGenerator : MonoBehaviour {
    [SerializeField, Range( 0, 9999.0f )] private float flPerlinScale = 48.0f;
    [SerializeField, Range( 0, 9999 )] private int flPerlinOctaves = 5;
    [SerializeField, Range( 0, 10 )] private float flPersistence = 1f;
    [SerializeField, Range( 0, 10 )] private float flLacunarity = 1f;
    [SerializeField, Range( 0, 10 )] private float flPerlinBaseAmplitude = 0.50f;
    [SerializeField, Range( 0, 320000 )] private int nSeed = 16275;

    [SerializeField] private TileBase[ ] pTiles = null;
    [SerializeField] private Material pWaterMaterial = null;
    [SerializeField] public Tilemap[ ] pMaps = null;
    [SerializeField] public float[ ] pMinTilesHeights = null;
    [SerializeField] public float[ ] pMaxTilesHeights = null;

    [SerializeField] private Transform pWizardTransform = null;

    public const int nChunkSize = 16;
    public const int nRenderDistance = 1;
    public const int nRenderDistanceSize = nRenderDistance * nChunkSize;
    public const int nChunksCountInLine = nRenderDistance * 2 + 1;
    public const int nChunksSizeInLine = nChunksCountInLine * nChunkSize;
    private Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    private Dictionary<Vector2Int, Chunk_t> aVisibleChunks = new Dictionary<Vector2Int, Chunk_t>( );
    private Texture2D pHeightMapTexture = null;
    private Vector2Int vStartChunk = Vector2Int.zero;
    private Vector2Int vPlayerChunkStart = Vector2Int.zero;

    private MapGeneratorHelper pHelper = new MapGeneratorHelper( );

    public void Init( ) {
        aAllChunks.Clear( );
        aVisibleChunks.Clear( );
    }
    private void Update( ) {
        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = pHelper.Rounded( this.pWizardTransform.transform.position.x / nChunkSize );
        vChunkPos.y = pHelper.Rounded( this.pWizardTransform.transform.position.y / nChunkSize );
        EnableChunks( vChunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * nChunkSize, vChunkPos.y * nChunkSize );
        if (!pHelper.PositionIsNew( vNewChunkPos ))
            return;

        vPlayerChunkStart = vNewChunkPos;

        pHelper.FillNearestChunksUsingRenderDistance( nRenderDistance );

        List<Chunk_t> vAddedChunks = new List<Chunk_t>( );
        foreach (Vector2Int vChunkOffset in pHelper.aNearestChunksPos) {
            Vector2Int nChunkStartPos = new Vector2Int(
                vNewChunkPos.x + vChunkOffset.x * nChunkSize,
                vNewChunkPos.y + vChunkOffset.y * nChunkSize );

            Chunk_t offsetChunk = GetChunk( nChunkStartPos.x, nChunkStartPos.y );
            if (!offsetChunk.bVisible) {
                offsetChunk.bVisible = true;
                vAddedChunks.Add( offsetChunk );
                this.aVisibleChunks.Add( offsetChunk.vPos, offsetChunk );
            }
        }

        List<Vector2Int> vRemovedChunksCoord = this.ClearFarChunks( this.aVisibleChunks, nRenderDistanceSize );
        this.ClearFarChunks( this.aAllChunks, nRenderDistanceSize * 2 );

        vStartChunk = pHelper.FindStartChunk( this.aVisibleChunks );

        this.pHeightMapTexture = pHelper.GenerateHeightMapTexture( this.aVisibleChunks, this.vStartChunk );
        if (this.pHeightMapTexture != null) {
            this.pHeightMapTexture.Apply( );

            pHelper.SetupMaterialData( pWaterMaterial, this.aVisibleChunks, this.pHeightMapTexture, this.vStartChunk );
        }

        this.ClearTileMaps( vRemovedChunksCoord );
        this.FillTiles( vAddedChunks );

        pHelper.vLastChunkPos = vNewChunkPos;
    }

    public void GenerateChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (!this.aAllChunks.ContainsKey( vChunkPos )) {
            Chunk_t pNewChunk = new Chunk_t( );

            pNewChunk.vPos = vChunkPos;
            pNewChunk.Heights = new float[ nChunkSize, nChunkSize ];

            MapGeneratorHelper.PerlinGeneratorSettings pSettings = new MapGeneratorHelper.PerlinGeneratorSettings {
                flPerlinScale = this.flPerlinScale,
                flPerlinOctaves = this.flPerlinOctaves,
                flPersistence = this.flPersistence,
                flLacunarity = this.flLacunarity,
                flPerlinBaseAmplitude = this.flPerlinBaseAmplitude
            };

            System.Random random = new System.Random( nSeed );
            pSettings.vRandomOffsets = new Vector2[ this.flPerlinOctaves ];
            for (int i = 0; i < this.flPerlinOctaves; i++)
                pSettings.vRandomOffsets[ i ] = new Vector2( random.Next( -10000, 10000 ), random.Next( -10000, 10000 ) );

            pHelper.FillChunkHeights( pNewChunk, pSettings );
            this.aAllChunks.Add( vChunkPos, pNewChunk );
        }
    }

    void FillTiles( List<Chunk_t> pAddedCoords ) {
        if (this.pMaps == null || this.pMaps.Length <= 0)
            return;

        foreach (Chunk_t pChunk in pAddedCoords) {
            TileBase[ ][ ] pTiles = new TileBase[ this.pMaps.Length ][ ];
            for (int nMapLayer = 0; nMapLayer < this.pMaps.Length; nMapLayer++) {
                pTiles[ nMapLayer ] = new TileBase[ nChunkSize * nChunkSize ];

                int i = 0;
                for (int y = 0; y < nChunkSize; y++) {
                    for (int x = 0; x < nChunkSize; x++) {

                        float flHeight = pChunk.Heights[ x, y ];
                        if (flHeight >= this.pMinTilesHeights[ nMapLayer ] && flHeight < this.pMaxTilesHeights[ nMapLayer ])
                            pTiles[ nMapLayer ][ i ] = this.pTiles[ nMapLayer ];
                        i++;
                    }
                }

                BoundsInt bounds = new BoundsInt(
                    pChunk.vPos.x, pChunk.vPos.y, 0,
                    nChunkSize, nChunkSize, 1
                );

                //pLayer0TileMap.SetTilesBlock( bounds, pTilesLayer0 );
                //pLayer1TileMap.SetTilesBlock( bounds, pTilesLayer1 );
                //pLayer2TileMap.SetTilesBlock( bounds, pTilesLayer2 );

                this.pMaps[ nMapLayer ].SetTilesBlock( bounds, pTiles[ nMapLayer ] );
            }
        }
    }
    public Chunk_t GetChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (this.aVisibleChunks.ContainsKey( vChunkPos )) {
            return this.aVisibleChunks[ vChunkPos ];
        } else {
            if (!this.aAllChunks.ContainsKey( vChunkPos ))
                GenerateChunk( x, y );

            return this.aAllChunks[ vChunkPos ];
        }
    }
    private void OnDrawGizmos( ) {
        foreach (var c in this.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.Value.vPos.x * nChunkSize, c.Value.vPos.y * nChunkSize );
            for (int x = 0; x < nChunkSize; x++) {
                for (int y = 0; y < nChunkSize; y++) {
                    Vector3 vCellPos = new Vector3( c.Value.vPos.x + x + 0.5f, c.Value.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Value.Heights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }
    }
    private void OnGUI( ) {
        /*return;
        Vector3 vMousePos = Input.mousePosition;
        Vector3 vWorldPosition = Camera.main.ScreenToWorldPoint( vMousePos );

        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = MapGeneratorHelper.Rounded( vWorldPosition.x / nChunkSize );
        vChunkPos.y = MapGeneratorHelper.Rounded( vWorldPosition.y / nChunkSize );

        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * nChunkSize, vChunkPos.y * nChunkSize );
        Chunk_t pChunk = GetChunk( vNewChunkPos.x, vNewChunkPos.y );

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
                nChunkSize, nChunkSize, 1
            );
            TileBase[ ] nullTiles = new TileBase[ nChunkSize * nChunkSize ];
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