using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
/*
TODO:
1) clear this.allchunks if distance > 3 * chunksize.
2) Do not rebuild chunks if it was wisible before.
*/
public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
}
public class MapGenerator : MonoBehaviour {
    public TileBase pSandTile = null;
    public TileBase pWaterTile = null;
    public TileBase pWaterShadowTile = null;
    public Material pWaterMaterial = null;
    public static int ChunkSize = 16;
    public static int ChunkCount = 2;
    public static int nChunksCountInLine = ChunkCount * 2 + 1;
    public static int nChunksSizeInLine = nChunksCountInLine * ChunkSize;
    public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    public List<Chunk_t> aVisibleChunks = new List<Chunk_t>( );
    private Texture2D pHeightMapTexture = null;
    public Tilemap pLayer1TileMap = null;
    public Tilemap pLayer2TileMap = null;
    public Tilemap pLayer0TileMap = null;

    Transform pWizardTransform = null;

    private void Start( ) {
        this.pWizardTransform = GameObject.Find( "Wizard" ).transform;

        aAllChunks.Clear( );
        aVisibleChunks.Clear( );
    }
    private void Update( ) {
        Vector2Int vChunkPos = new Vector2Int( );
        vChunkPos.x = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.x / ChunkSize );
        vChunkPos.y = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.y / ChunkSize );
        EnableChunks( vChunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkSize, vChunkPos.y * ChunkSize );
        if (!MapGeneratorHelper.PositionIsNew( vNewChunkPos ))
            return;

        MapGeneratorHelper.aNearestChunksPos.Clear( );

        for (int x = -ChunkCount; x <= ChunkCount; x++) {
            for (int y = -ChunkCount; y <= ChunkCount; y++) {
                MapGeneratorHelper.aNearestChunksPos.Add( new Vector2Int( x, y ) );
            }
        }

        this.aVisibleChunks.Clear( );
        foreach (Vector2Int vChunkOffset in MapGeneratorHelper.aNearestChunksPos) {
            Chunk_t offsetChunk = GetChunk(
                vNewChunkPos.x + vChunkOffset.x * ChunkSize,
                vNewChunkPos.y + vChunkOffset.y * ChunkSize );

            offsetChunk.bVisible = true;
            this.aVisibleChunks.Add( offsetChunk );
        }

        this.pHeightMapTexture = MapGeneratorHelper.GenerateHeightMapTexture( this.aVisibleChunks );
        if (this.pHeightMapTexture != null) {
            this.pHeightMapTexture.Apply( );

            MapGeneratorHelper.SetupMaterialData( pWaterMaterial, this.aVisibleChunks, this.pHeightMapTexture );
        }

        this.ClearTileMaps( );
        this.FillTiles( );

        MapGeneratorHelper.vLastChunkPos = vNewChunkPos;
    }

    public void GenerateChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (!this.aAllChunks.ContainsKey( vChunkPos )) {
            Chunk_t pNewChunk = new Chunk_t( );

            pNewChunk.vPos = vChunkPos;
            pNewChunk.Heights = new float[ ChunkSize, ChunkSize ];
            MapGeneratorHelper.FillChunkHeights( pNewChunk );
            this.aAllChunks.Add( vChunkPos, pNewChunk );
        }
    }

    void FillTiles( ) {
        if (!pLayer1TileMap || !pLayer2TileMap || !pLayer0TileMap)
            return;

        foreach (Chunk_t pChunk in this.aVisibleChunks) {

            int i = 0;
            TileBase[ ] pTilesLayer0 = new TileBase[ ChunkSize * ChunkSize ];
            TileBase[ ] pTilesLayer1 = new TileBase[ ChunkSize * ChunkSize ];
            TileBase[ ] pTilesLayer2 = new TileBase[ ChunkSize * ChunkSize ];
            for (int y = 0; y < ChunkSize; y++) {
                for (int x = 0; x < ChunkSize; x++) {

                    float flHeight = pChunk.Heights[ x, y ];
                    if (flHeight > 0.5f) {
                        pTilesLayer2[ i ] = pSandTile;
                    } else {
                        pTilesLayer0[ i ] = pWaterShadowTile;
                        pTilesLayer1[ i ] = pWaterTile;
                        pTilesLayer2[ i ] = pSandTile;
                    }

                    BoundsInt bounds = new BoundsInt(
                        pChunk.vPos.x,
                        pChunk.vPos.y,
                        0,
                        ChunkSize,
                        ChunkSize,
                        1
                    );

                    pLayer0TileMap.SetTilesBlock( bounds, pTilesLayer0 );
                    pLayer1TileMap.SetTilesBlock( bounds, pTilesLayer1 );
                    pLayer2TileMap.SetTilesBlock( bounds, pTilesLayer2 );

                    i++;
                }
            }
        }
    }
    private void ClearTileMaps( ) {
        this.pLayer0TileMap.ClearAllTiles( );
        this.pLayer1TileMap.ClearAllTiles( );
        this.pLayer2TileMap.ClearAllTiles( );
    }
    public Chunk_t GetChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (this.aAllChunks.ContainsKey( vChunkPos )) {
            return this.aAllChunks[ vChunkPos ];
        } else {
            GenerateChunk( x, y );
            return this.aAllChunks[ vChunkPos ];
        }
    }
    private void OnDrawGizmos( ) {
        foreach (Chunk_t c in this.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.vPos.x * ChunkSize, c.vPos.y * ChunkSize );
            for (int x = 0; x < ChunkSize; x++) {
                for (int y = 0; y < ChunkSize; y++) {
                    Vector3 vCellPos = new Vector3( c.vPos.x + x + 0.5f, c.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Heights[ x, y ];
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
        vChunkPos.x = MapGeneratorHelper.Rounded( vWorldPosition.x / ChunkSize );
        vChunkPos.y = MapGeneratorHelper.Rounded( vWorldPosition.y / ChunkSize );

        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkSize, vChunkPos.y * ChunkSize );
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
}