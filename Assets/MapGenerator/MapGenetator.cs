using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Tilemaps;

public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
}
public class MapGenetator : MonoBehaviour {
    public TileBase pSandTile = null;
    public TileBase pWaterTile = null;
    public Material pWaterMaterial = null;
    public static int ChunkSize = 16;
    public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    public List<Chunk_t> aVisibleChunks = new List<Chunk_t>( );
    private Texture2D pHeightMapTexture = null;
    public Tilemap pLayer1TileMap = null;
    public Tilemap pLayer2TileMap = null;

    Transform pWizardTransform = null;

    private void Start( ) {
        this.pWizardTransform = GameObject.Find("Wizard").transform;

        aAllChunks.Clear( );
        aVisibleChunks.Clear( );
    }
    private void Update( ) {
        Vector2Int chunkPos = new Vector2Int( );
        chunkPos.x = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.x / ChunkSize );
        chunkPos.y = MapGeneratorHelper.Rounded( this.pWizardTransform.transform.position.y / ChunkSize );
        EnableChunks( chunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkSize, vChunkPos.y * ChunkSize );
        if (!MapGeneratorHelper.PositionIsNew( vNewChunkPos ))
            return;

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

        this.pLayer1TileMap.ClearAllTiles( );
        this.pLayer2TileMap.ClearAllTiles( );
        foreach (Chunk_t pChunk in this.aVisibleChunks)
            this.FillTiles( pChunk );

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

    void FillTiles( Chunk_t pChunk ) {
        if (!pLayer1TileMap || !pLayer2TileMap)
            return;

        TileBase[ ] pTilesLayer1 = new TileBase[ ChunkSize * ChunkSize ];
        TileBase[ ] pTilesLayer2 = new TileBase[ ChunkSize * ChunkSize ];
        int LocalX = 0;
        int LocalY = 0;
        for (int i = 0 ; i < pTilesLayer1.Length ; i++) {
            if (LocalY > ChunkSize - 1) {
                LocalX += 1;
                LocalY = 0;
            }

            float flHeight = pChunk.Heights[ LocalX, LocalY ];
            if (flHeight > 0.5f) {
                pTilesLayer2[ i ] = pSandTile;
            } else {
                pTilesLayer1[ i ] = pWaterTile;
                pTilesLayer2[ i ] = pSandTile;
            }
            LocalY++;
        }
        BoundsInt bounds = new BoundsInt(
            pChunk.vPos.x,
            pChunk.vPos.y,
            0,
            ChunkSize,
            ChunkSize,
            1
        );

        pLayer1TileMap.SetTilesBlock( bounds, pTilesLayer1 );
        pLayer2TileMap.SetTilesBlock( bounds, pTilesLayer2 );
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

        //Vector3 vWorldPosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        
        //Gizmos.DrawCube

        /*foreach (Chunk_t c in this.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.vPos.x * ChunkSize, c.vPos.y * ChunkSize );
            for (int x = 0 ; x < ChunkSize ; x++) {
                for (int y = 0 ; y < ChunkSize ; y++) {
                    Vector3 vCellPos = new Vector3( c.vPos.x + x + 0.5f, c.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Heights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }*/
    }
}