using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.Tilemaps;

public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
    public Texture2D pHeightTexture;
    public Tilemap pLayer1TileMap;
    public Tilemap pLayer2TileMap;
    public GameObject gameObject;
}

public class MapGenetator : MonoBehaviour {
    public TileBase pSandTile = null;
    public TileBase pWaterTile = null;
    public Material pWaterMaterial = null;
    public int ChunkSize = 16;
    public float PerlinScale = 48.0f;
    public int PerlinOctaves = 5;
    public float persistence = 1f;
    public float lacunarity = 1f;
    public float PerlinBaseAmplitude = 0.52f;
    public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    public List<Chunk_t> aVisibleChunks = new List<Chunk_t>( );

    public BasePlayer pPlayer = null;

    private Vector2Int vLastChunkPos = Vector2Int.zero;

    private List<Vector2Int> aNearestChunksPos = new List<Vector2Int> {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.one,
            -Vector2Int.one,
            new Vector2Int( 1, -1 ),
            new Vector2Int( -1, 1 ),
            Vector2Int.zero
        };

    private void Start( ) {
        if (!pPlayer)
            return;

        aAllChunks.Clear( );
        aVisibleChunks.Clear( );

        foreach (Vector2Int vChunkOffset in aNearestChunksPos) {
            Vector2Int vRealChunkPos = vChunkOffset * ChunkSize;
            GenerateChunk( vRealChunkPos.x, vRealChunkPos.y );
            Chunk_t pPreviewChank = GetChunk( vRealChunkPos.x, vRealChunkPos.y );
            pPreviewChank.bVisible = true;
            this.aVisibleChunks.Add( pPreviewChank );
        }
    }
    public int Rounded( float flFrom ) {
        int nReturn = 0;

        if (flFrom > 0.0f) {
            nReturn = (int)Math.Floor( flFrom );
        } else {
            nReturn = -(int)Math.Ceiling( Math.Abs( flFrom ) );
        }

        return nReturn;
    }
    private void Update( ) {
        Vector2Int chunkPos = new Vector2Int( );
        chunkPos.x = (int)Rounded( pPlayer.transform.position.x / ChunkSize );
        chunkPos.y = (int)Rounded( pPlayer.transform.position.y / ChunkSize );
        EnableChunks( chunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkSize, vChunkPos.y * ChunkSize );
        if (!PositionIsNew( vNewChunkPos ))
            return;

        Debug.Log( "Chunks Updating!" );
        //foreach (Chunk_t chunk in this.aVisibleChunks) {
        //    Destroy( chunk.gameObject );
        //}
        this.aVisibleChunks.Clear( );
        foreach (Vector2Int vChunkOffset in aNearestChunksPos) {
            Chunk_t offsetChunk = GetChunk(
                vNewChunkPos.x + vChunkOffset.x * ChunkSize,
                vNewChunkPos.y + vChunkOffset.y * ChunkSize );

            offsetChunk.bVisible = true;
            this.aVisibleChunks.Add( offsetChunk );
        }
        vLastChunkPos = vNewChunkPos;
    }

    public bool PositionIsNew( Vector2 position ) {
        return (vLastChunkPos != position);
    }

    private void CreateChunkObject( Chunk_t pChunk ) {
        pChunk.gameObject = new GameObject( );
        pChunk.gameObject.name = "Chunk{" + pChunk.vPos.x + "," + pChunk.vPos.y + "}";
        pChunk.gameObject.transform.SetParent( gameObject.transform );

        GameObject TileMapLayer1 = new GameObject( "TileMapLayer1" );
        GameObject TileMapLayer2 = new GameObject( "TileMapLayer2" );

        TileMapLayer1.transform.SetParent( pChunk.gameObject.transform );
        TileMapLayer2.transform.SetParent( pChunk.gameObject.transform );

        pChunk.pLayer1TileMap = TileMapLayer1.AddComponent<Tilemap>( );
        {
            TilemapRenderer pRenderer = TileMapLayer1.AddComponent<TilemapRenderer>( );
            pRenderer.material = pWaterMaterial;

            Vector2 vMapPos = pChunk.vPos;
            pRenderer.material.SetTexture( "_HeightMap", pChunk.pHeightTexture );
            pRenderer.material.SetFloat( "_CurrentWorldTextureScale", 1.0f / ChunkSize );
            pRenderer.material.SetVector( "_CurrentWorldTexturePos", vMapPos );
        }
        pChunk.pLayer2TileMap = TileMapLayer2.AddComponent<Tilemap>( );
        {
            TileMapLayer2.AddComponent<TilemapRenderer>( );
        }
    }

    public void GenerateChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (!this.aAllChunks.ContainsKey( vChunkPos )) {
            Chunk_t pNewChunk = new Chunk_t( );

            pNewChunk.vPos = vChunkPos;
            pNewChunk.Heights = new float[ ChunkSize, ChunkSize ];
            this.GenerateChunkData( pNewChunk );

            pNewChunk.pHeightTexture = this.GenerateHeightMapTexture( pNewChunk );
            if (pNewChunk.pHeightTexture) {
                pNewChunk.pHeightTexture.Apply( );
            }

            pNewChunk.gameObject = new GameObject( );
            pNewChunk.gameObject.name = "Chunk{" + pNewChunk.vPos.x + "," + pNewChunk.vPos.y + "}";
            pNewChunk.gameObject.transform.SetParent( gameObject.transform );

            GameObject TileMapLayer1 = new GameObject( "TileMapLayer1" );
            GameObject TileMapLayer2 = new GameObject( "TileMapLayer2" );

            TileMapLayer1.transform.SetParent( pNewChunk.gameObject.transform );
            TileMapLayer2.transform.SetParent( pNewChunk.gameObject.transform );

            pNewChunk.pLayer1TileMap = TileMapLayer1.AddComponent<Tilemap>( );
            {
                TilemapRenderer pRenderer = TileMapLayer1.AddComponent<TilemapRenderer>( );
                pRenderer.material = pWaterMaterial;

                Vector2 vMapPos = pNewChunk.vPos;
                pRenderer.material.SetTexture( "_HeightMap", pNewChunk.pHeightTexture );
                pRenderer.material.SetFloat( "_CurrentWorldTextureScale", 1.0f / ChunkSize );
                pRenderer.material.SetVector( "_CurrentWorldTexturePos", vMapPos );
            }
            pNewChunk.pLayer2TileMap = TileMapLayer2.AddComponent<Tilemap>( );
            {
                TilemapRenderer pRenderer = TileMapLayer2.AddComponent<TilemapRenderer>( );
                TileMapLayer2.transform.position = new Vector3(0, 0, 0.1f);
            }

            this.FillTiles( pNewChunk );
            this.aAllChunks.Add( vChunkPos, pNewChunk );
        }
    }

    void FillTiles( Chunk_t pChunk ) {
        if (!pChunk.pLayer1TileMap || !pChunk.pLayer2TileMap)
            return;

        pChunk.pLayer1TileMap.ClearAllTiles( );
        pChunk.pLayer2TileMap.ClearAllTiles( );

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
            if (flHeight > 0.65f) {
                pTilesLayer2[ i ] = pSandTile;
            } else {
                pTilesLayer1[ i ] = pWaterTile;
                pTilesLayer2[ i ] = pSandTile;
            }

            /*Vector3Int vLocalPos = new Vector3Int( pChunk.vPos.x + LocalX, pChunk.vPos.y + LocalY, 0 );
            Color cPixel = new Color( flHeight, flHeight, flHeight, 1 );
            this.pLayer2TileMap.SetTileFlags( vLocalPos, TileFlags.None );
            this.pLayer2TileMap.SetColor( vLocalPos, cPixel );*/
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

        pChunk.pLayer1TileMap.SetTilesBlock( bounds, pTilesLayer1 );
        pChunk.pLayer2TileMap.SetTilesBlock( bounds, pTilesLayer2 );
    }

    public Chunk_t GetChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (this.aAllChunks.ContainsKey( vChunkPos )) {
            //FillTiles( this.aAllChunks[ vChunkPos ] );
            return this.aAllChunks[ vChunkPos ];
        } else {
            GenerateChunk( x, y );
            return this.aAllChunks[ vChunkPos ];
        }
    }

    private void GenerateChunkData( Chunk_t pChunk ) {
        for (int x = 0 ; x < ChunkSize ; x++) {
            for (int y = 0 ; y < ChunkSize ; y++) {
                float amplitude = PerlinBaseAmplitude;
                float freq = 1;
                float noiseHeight = 0;

                for (int i = 0 ; i < PerlinOctaves ; i++) {
                    float px = (pChunk.vPos.x + x) / PerlinScale * freq;
                    float py = (pChunk.vPos.y + y) / PerlinScale * freq;

                    float PerlinValue = Mathf.PerlinNoise( px, py ) * 2 - 1;
                    noiseHeight += PerlinValue * amplitude;

                    amplitude *= persistence;
                    freq *= lacunarity;
                }
                noiseHeight = Mathf.InverseLerp( -1f, 1f, noiseHeight );
                pChunk.Heights[ x, y ] = noiseHeight;
            }
        }
    }
    public Texture2D GenerateHeightMapTexture( Chunk_t pChunk ) {
        Texture2D heightMap = new Texture2D( ChunkSize, ChunkSize );
        for (int i = 0 ; i < heightMap.width ; i++) {
            for (int j = 0 ; j < heightMap.width ; j++) {
                float height = pChunk.Heights[ i, j ];
                Color colour = new Color( height, height, height, 1 );
                heightMap.SetPixel( i, j, colour );
            }
        }
        return heightMap;
    }
    private void OnDrawGizmos( ) {
        foreach (Chunk_t c in this.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.vPos.x * ChunkSize, c.vPos.y * ChunkSize );
            for (int x = 0 ; x < ChunkSize ; x++) {
                for (int y = 0 ; y < ChunkSize ; y++) {
                    Vector3 vCellPos = new Vector3( c.vPos.x + x + 0.5f, c.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Heights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }
    }
}

/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileData_t {
    public float nTemperature;
    public float nHeight;
}

public class MapGenetator : MonoBehaviour {

    public Material pWaterMaterial = null;
    public Material pSandMaterial = null;
    public Material pGroundMaterial = null;

    public int nMapSize = 256;
    public Tilemap pGroundTileMap = null;
    public Tilemap pSandTileMap = null;
    public Tilemap pWaterTileMap = null;

    public TileBase pSandTile = null;
    public TileBase pGroundTile = null;
    public TileBase pWaterTile = null;

    private TileData_t[ , ] pTerrainMap;

    private int seed = 698512;
    public int octaves = 20;
    private float lacunarity = 1;
    private float persistance = 1;
    public float scale = 75;
    private float flWaterSmooth = 0.145f;

    public void Start( ) {
        if (pGroundTileMap == null || pWaterTileMap == null || pSandTileMap == null)
            return;

        this.GenerateMap( );
        this.transform.position = new Vector3( -nMapSize / 2, -nMapSize / 2, 0 );
    }

    public void CreateHeightMap( ) {
        this.pTerrainMap = new TileData_t[ nMapSize, nMapSize ];

        for (int i = 0 ; i < this.pTerrainMap.GetLength( 0 ) ; i++)
            for (int j = 0 ; j < this.pTerrainMap.GetLength( 1 ) ; j++)
                this.pTerrainMap[ i, j ] = GenerateTileData( i, j, seed, octaves, lacunarity, persistance, scale );

        Texture2D pTexture = GenerateHeightMapTexture( );
        if (pTexture != null) {
            pTexture.Apply( );

            Vector2 vMapPos = new Vector2( -nMapSize / 2, -nMapSize / 2 );

            pWaterMaterial.SetTexture( "_HeightMap", pTexture );
            pWaterMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / nMapSize );
            pWaterMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );

            pSandMaterial.SetTexture( "_HeightMap", pTexture );
            pSandMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / nMapSize );
            pSandMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );

            pGroundMaterial.SetTexture( "_HeightMap", pTexture );
            pGroundMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / nMapSize );
            pGroundMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );
        }
    }

    public void GenerateMap( ) {
        this.ClearMap( );
        this.CreateHeightMap( );

        float nMaxHeight = int.MinValue;
        float nMinHeight = int.MaxValue;

        float nMaxTe = int.MinValue;
        float nMinTe = int.MaxValue;

        for (int i = 0 ; i < pTerrainMap.GetLength( 0 ) ; i++) {
            for (int j = 0 ; j < pTerrainMap.GetLength( 1 ) ; j++) {
                Vector3Int vNewTilePos = new Vector3Int( i, j, 0 );
                TileData_t pTileData = this.pTerrainMap[ i, j ];

                if (pTileData.nHeight > 0.45f) {
                    //ground, mountains, forest etc... [#TODO:Terrain]
                    if (pTileData.nHeight < 0.47) {
                        pSandTileMap.SetTile( vNewTilePos, pSandTile );
                    } else {
                        pGroundTileMap.SetTile( vNewTilePos, pGroundTile );
                    }
                } else {
                    //UnderGround (Sea, Caves) [#TODO:Terrain]
                    pSandTileMap.SetTile( vNewTilePos, pSandTile );
                    pWaterTileMap.SetTile( vNewTilePos, pWaterTile );
                }

                if (pTileData.nHeight > nMaxHeight)
                    nMaxHeight = pTileData.nHeight;
                if (pTileData.nHeight < nMinHeight)
                    nMinHeight = pTileData.nHeight;

                if (pTileData.nTemperature > nMaxTe)
                    nMaxTe = pTileData.nTemperature;
                if (pTileData.nTemperature < nMinTe)
                    nMinTe = pTileData.nTemperature;
            }
        }

        Debug.Log( nMaxHeight );
        Debug.Log( nMinHeight );
        Debug.Log( nMaxTe );
        Debug.Log( nMinTe );
    }

    public TileData_t GenerateTileData( int x, int y, int seed, int octaves, float lacunarity, float persistance, float scale ) {
        TileData_t pTileData = new TileData_t( );

        System.Random rnd = new System.Random( seed );

        Vector2[ ] octOffsets = new Vector2[ octaves ];

        for (int i = 0 ; i < octaves ; i++) {
            float offsetX = rnd.Next( -500, 500 );
            float offsetY = rnd.Next( -500, 500 );
            octOffsets[ i ] = new Vector2( offsetX, offsetY );
        }

        float flFrequency = 1;
        float flHeight = 0;
        float flAmplitude = 1;
        float flTemperature = 0.0f;

        for (int i = 0 ; i < octaves ; i++) {
            float sampleX = x / (scale / flFrequency) + octOffsets[ i ].x + 0.1f;
            float sampleY = y / (scale / flFrequency) + octOffsets[ i ].y + 0.1f;

            float perlinValue = Mathf.PerlinNoise( sampleX, sampleY );
            flHeight += perlinValue * flAmplitude;
            flTemperature += perlinValue * flAmplitude;
            flAmplitude *= persistance;
            flFrequency *= lacunarity;
        }

        flHeight /= octaves;

        pTileData.nHeight = flHeight;
        pTileData.nTemperature = flTemperature;

        return pTileData;
    }

    public Texture2D GenerateHeightMapTexture( ) {
        Texture2D heightMap = new Texture2D( nMapSize, nMapSize );
        for (int i = 0 ; i < heightMap.width ; i++) {
            for (int j = 0 ; j < heightMap.width ; j++) {
                float height = pTerrainMap[ i, j ].nHeight - this.flWaterSmooth;
                Color colour = new Color( height, height, height, 1 );
                if (height <= 0.15f)
                    colour = Color.black;

                heightMap.SetPixel( i, j, colour );
            }
        }
        return heightMap;
    }

    private void Update( ) {
        Vector3Int vMousePos = new Vector3Int(
            (int)Input.mousePosition.x,
            (int)Input.mousePosition.y,
            (int)Input.mousePosition.z );
        var mouseWorldPos = Camera.main.ScreenToWorldPoint( vMousePos );

        TileData_t pTile = pTerrainMap[ nMapSize / 2 + (int)mouseWorldPos.x, nMapSize / 2 + (int)mouseWorldPos.y ];
        Debug.Log( pTile.nHeight );
    }
    public void ClearMap( ) {
        pGroundTileMap.ClearAllTiles( );
        pWaterTileMap.ClearAllTiles( );
        pSandTileMap.ClearAllTiles( );
    }
}
*/