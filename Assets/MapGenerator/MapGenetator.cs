using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileData_t {
    public float nTemperature;
    public float nHeight;
}

public class MapGenetator : MonoBehaviour {

    public Material pWaterMaterial = null;

    public int nWidth = 250;
    public int nHeight = 250;
    public Tilemap pGroundTileMap = null;
    public Tilemap pSandTileMap = null;
    public Tilemap pWaterTileMap = null;
    public Tilemap pWaterShadowTileMap = null;

    public TileBase pSandTile = null;
    public TileBase pGroundTile = null;
    public TileBase pWaterTile = null;
    public TileBase pWaterShadowTile = null;

    public TileData_t[ , ] pTerrainMap;

    public int seed = 698512;
    public int octaves = 20;
    public float lacunarity = 1;
    public float persistance = 1;
    public float scale = 75;
    public float flWaterSmooth = 0.145f;

    public int nGenerationDecorChance = 15;
    public void Start( ) {
        if (pGroundTileMap == null || pWaterShadowTileMap == null || pWaterTileMap == null)
            return;

        this.GenerateMap( );
        this.transform.position -= new Vector3( nWidth / 2, nHeight / 2, 0 );
    }

    public void CreateHeightMap( ) {
        this.pTerrainMap = new TileData_t[ nWidth, nHeight ];

        for (int i = 0 ; i < this.pTerrainMap.GetLength( 0 ) ; i++)
            for (int j = 0 ; j < this.pTerrainMap.GetLength( 1 ) ; j++)
                this.pTerrainMap[ i, j ] = GenerateTileData( i, j, seed, octaves, lacunarity, persistance, scale );

        Texture2D pTexture = GenerateHeightMapTexture( );
        if (pTexture != null) {
            pTexture.Apply( );
            pWaterMaterial.SetTexture( "_HeightMap", pTexture );
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

                if (pTileData.nHeight > 0.50f) {
                    //ground, mountains, forest etc... [#TODO:Terrain]
                    if (pTileData.nHeight < 0.52 || pTileData.nTemperature > 30) {
                        pSandTileMap.SetTile( vNewTilePos, pSandTile );
                    } else if (pTileData.nHeight < 0.70) {
                        pGroundTileMap.SetTile( vNewTilePos, pGroundTile );
                    } else if (pTileData.nHeight >= 0.70) {
                        //Mountain [#TODO]
                        pGroundTileMap.SetTile( vNewTilePos, pGroundTile );
                    }
                } else {
                    //UnderGround (Sea, Caves) [#TODO:Terrain]

                    pSandTileMap.SetTile( vNewTilePos, pSandTile );
                    pWaterTileMap.SetTile( vNewTilePos, pWaterTile );

                    //if (pTileData.nHeight <= -0.17f) {
                    //    pWaterShadowTileMap.SetTile( vNewTilePos, pWaterShadowTile );
                    //}
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
        Texture2D heightMap = new Texture2D( nWidth, nHeight );
        for (int i = 0 ; i < heightMap.width ; i++) {
            for (int j = 0 ; j < heightMap.width ; j++) {
                float height = pTerrainMap[ i, j ].nHeight - this.flWaterSmooth;
                Color colour = new Color( height, height, height, 1 );
                heightMap.SetPixel( i, j, colour );
            }
        }

        return heightMap;
    }

    public void ClearMap( ) {
        pGroundTileMap.ClearAllTiles( );
        pWaterTileMap.ClearAllTiles( );
        pSandTileMap.ClearAllTiles( );
        pWaterShadowTileMap.ClearAllTiles( );
    }
}