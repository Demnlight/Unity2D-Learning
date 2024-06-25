using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

struct Chunk_t {
    public Vector2Int vPos;
    public Vector2Int vSize;
    public float[ , ] aHeights;
    public Texture2D pHeightTexture;
}

public class MapGenetator : MonoBehaviour {

    public Material pWaterMaterial = null;
    public Material pSandMaterial = null;

    public Tilemap pSandTileMap = null;
    public Tilemap pWaterTileMap = null;

    public TileBase pSandTile = null;
    public TileBase pWaterTile = null;

    private List<Chunk_t> aActiveChunks = new List<Chunk_t>( );
    public int nChunkSize = 16;

    public void Start( ) {
        if (pWaterTileMap == null || pSandTileMap == null)
            return;

        for (int x = 0 ; x < 4 ; x++) {
            for (int y = 0 ; y > -4 ; y++) {
                this.aActiveChunks.Add( CreateChunk( x * nChunkSize, y * nChunkSize ) );
            }
        }
    }
    private Chunk_t CreateChunk( int nStartX, int nStartY ) {
        Chunk_t pNewChunk = new Chunk_t( );
        pNewChunk.vPos = new Vector2Int( nStartX, nStartY );
        pNewChunk.vSize = new Vector2Int( nChunkSize, nChunkSize );

        pNewChunk.aHeights = new float[ nChunkSize, nChunkSize ];
        for (int x = 0 ; x < pNewChunk.aHeights.GetLength( 0 ) ; x++) {
            for (int y = 0 ; y < pNewChunk.aHeights.GetLength( 1 ) ; y++) {
                pNewChunk.aHeights[ x, y ] = GetMapHeight( x, y, pNewChunk.vPos );
            }
        }
        this.GenerateHeightMap( pNewChunk );
        return pNewChunk;
    }
    private float GetMapHeight( int x, int y, Vector2Int vChunkPos ) {
        float flHeight = 0.0f;

        float flSampleX = ((float)x) / nChunkSize;
        float flSampleY = ((float)y) / nChunkSize;
        flHeight += Mathf.PerlinNoise( flSampleX, flSampleY );
        return flHeight;
    }
    private void GenerateHeightMap( Chunk_t pChunk ) {
        pChunk.pHeightTexture = new Texture2D( pChunk.vSize.x, pChunk.vSize.y );
        for (int x = 0 ; x < pChunk.pHeightTexture.width ; x++) {
            for (int y = 0 ; y < pChunk.pHeightTexture.height ; y++) {
                float flHeight = pChunk.aHeights[ x, y ];
                Color cPixel = new Color( flHeight, flHeight, flHeight, 1 );
                pChunk.pHeightTexture.SetPixel( x, y, cPixel );
            }
        }
        pChunk.pHeightTexture.Apply( );
    }
    /*public void CreateHeightMap( ) {
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
                    pSandTileMap.SetTile( vNewTilePos, pSandTile );
                } else {
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
        pWaterTileMap.ClearAllTiles( );
        pSandTileMap.ClearAllTiles( );
    }*/
}