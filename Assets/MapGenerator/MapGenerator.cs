using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;
using Unity.VisualScripting;
/*
TODO:
1) clear this.allchunks if distance > 3 * nChunkSize.
2) Do not rebuild chunks if it was wisible before.
*/
public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
}
public class MapGenerator : MonoBehaviour {
    [SerializeField] private TileBase pSandTile = null;
    [SerializeField] private TileBase pWaterTile = null;
    [SerializeField] private TileBase pWaterShadowTile = null;
    [SerializeField] private Material pWaterMaterial = null;
    [SerializeField] public Tilemap pLayer0TileMap = null;
    [SerializeField] public Tilemap pLayer1TileMap = null;
    [SerializeField] public Tilemap pLayer2TileMap = null;

    public static int nChunkSize = 16;
    public static int nRenderDistance = 1;
    public static int nChunksCountInLine = nRenderDistance * 2 + 1;
    public static int nChunksSizeInLine = nChunksCountInLine * nChunkSize;
    private Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    private Dictionary<Vector2Int, Chunk_t> aVisibleChunks = new Dictionary<Vector2Int, Chunk_t>( );
    private Texture2D pHeightMapTexture = null;
    private Transform pWizardTransform = null;
    public static Vector2Int vStartChunk = Vector2Int.zero;
    private Vector2Int vPlayerChunkStart = Vector2Int.zero;
    private void Start( ) {
        this.pWizardTransform = GameObject.Find( "Wizard" ).transform;

        aAllChunks.Clear( );
        aVisibleChunks.Clear( );
    }
    private void Update( ) {
        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.x / nChunkSize );
        vChunkPos.y = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.y / nChunkSize );
        EnableChunks( vChunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * nChunkSize, vChunkPos.y * nChunkSize );
        if (!MapGeneratorHelper.PositionIsNew( vNewChunkPos ))
            return;

        vPlayerChunkStart = vNewChunkPos;

        MapGeneratorHelper.FillNearestChunksUsingRenderDistance( nRenderDistance );

        List<Chunk_t> vAddedChunks = new List<Chunk_t>( );
        List<Vector2Int> vToRemoveCoords = new List<Vector2Int>( );

        foreach (Vector2Int vChunkOffset in MapGeneratorHelper.aNearestChunksPos) {
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

        vToRemoveCoords = this.ClearFarChunks( this.aVisibleChunks );

        vStartChunk = MapGeneratorHelper.FindStartChunk( this.aVisibleChunks );

        this.pHeightMapTexture = MapGeneratorHelper.GenerateHeightMapTexture( this.aVisibleChunks );
        if (this.pHeightMapTexture != null) {
            this.pHeightMapTexture.Apply( );

            MapGeneratorHelper.SetupMaterialData( pWaterMaterial, this.aVisibleChunks, this.pHeightMapTexture );
        }

        this.ClearTileMaps( vToRemoveCoords );
        this.FillTiles( vAddedChunks );

        MapGeneratorHelper.vLastChunkPos = vNewChunkPos;
    }

    public void GenerateChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (!this.aAllChunks.ContainsKey( vChunkPos )) {
            Chunk_t pNewChunk = new Chunk_t( );

            pNewChunk.vPos = vChunkPos;
            pNewChunk.Heights = new float[ nChunkSize, nChunkSize ];
            MapGeneratorHelper.FillChunkHeights( pNewChunk );
            this.aAllChunks.Add( vChunkPos, pNewChunk );
        }
    }

    void FillTiles( List<Chunk_t> pAddedCoords ) {
        if (!pLayer1TileMap || !pLayer2TileMap || !pLayer0TileMap)
            return;

        foreach (Chunk_t pChunk in pAddedCoords) {

            int i = 0;
            TileBase[ ] pTilesLayer0 = new TileBase[ nChunkSize * nChunkSize ];
            TileBase[ ] pTilesLayer1 = new TileBase[ nChunkSize * nChunkSize ];
            TileBase[ ] pTilesLayer2 = new TileBase[ nChunkSize * nChunkSize ];

            for (int y = 0; y < nChunkSize; y++) {
                for (int x = 0; x < nChunkSize; x++) {

                    float flHeight = pChunk.Heights[ x, y ];
                    if (flHeight > 0.5f) {
                        pTilesLayer2[ i ] = pSandTile;
                    } else {
                        pTilesLayer0[ i ] = pWaterShadowTile;
                        pTilesLayer1[ i ] = pWaterTile;
                        //pTilesLayer2[ i ] = pSandTile;
                    }
                    i++;
                }
            }

            BoundsInt bounds = new BoundsInt(
                        pChunk.vPos.x, pChunk.vPos.y, 0,
                        nChunkSize, nChunkSize, 1
                    );

            pLayer0TileMap.SetTilesBlock( bounds, pTilesLayer0 );
            pLayer1TileMap.SetTilesBlock( bounds, pTilesLayer1 );
            pLayer2TileMap.SetTilesBlock( bounds, pTilesLayer2 );

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

    private void ClearTileMaps( List<Vector2Int> vClearedChunksPos ) {
        foreach (var v in vClearedChunksPos) {
            BoundsInt bounds = new BoundsInt(
                v.x, v.y, 0,
                nChunkSize, nChunkSize, 1
            );
            TileBase[ ] nullTiles = new TileBase[ nChunkSize * nChunkSize ];
            this.pLayer0TileMap.SetTilesBlock( bounds, nullTiles );
            this.pLayer1TileMap.SetTilesBlock( bounds, nullTiles );
            this.pLayer2TileMap.SetTilesBlock( bounds, nullTiles );
        }
    }

    private bool IsChunkFar( Vector2Int vChunkPos ) {
        int nXDistance = Math.Abs( vChunkPos.x - this.vPlayerChunkStart.x );
        int nYDistance = Math.Abs( vChunkPos.y - this.vPlayerChunkStart.y );
        bool bXOutOfDistance = nXDistance > nChunkSize * nRenderDistance;
        bool bYOutOfDistance = nYDistance > nChunkSize * nRenderDistance;

        return bXOutOfDistance || bYOutOfDistance;
    }
    private List<Vector2Int> ClearFarChunks( Dictionary<Vector2Int, Chunk_t> pChunks ) {
        List<Vector2Int> vToRemoveCoords = new List<Vector2Int>( );
        foreach (var pChunk in pChunks) {
            if (this.IsChunkFar( pChunk.Value.vPos ))
                vToRemoveCoords.Add( pChunk.Value.vPos );
        }

        foreach (Vector2Int vCoords in vToRemoveCoords) {
            pChunks.Remove( vCoords );
        }

        return vToRemoveCoords;
    }
}