using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileData_t {
    public int nTemperature;
    public int nHeight;
}

public class MapGenetator : MonoBehaviour {
    public int nWidth = 0;
    public int nHeight = 0;
    public Tilemap pGroundTileMap = null;
    public Tilemap pWaterTileMap = null;
    public Tilemap pWaterShadowTileMap = null;

    public TileBase[ ] pGroundTiles = null;
    public Tile[ ] pMountainsTiles = null;
    public Tile[ ] pGroundDecorTiles = null;
    //public Tile[ ] pGroundDecorTiles = null;
    public TileBase[ ] pWaterTiles = null;
    public TileBase pWaterShadowTile = null;
    public int[ , ] pTerrainMap;

    public int seed;
    public int octaves;
    public float lacunarity;
    public float persistance;
    public float scale;

    public int nGenerationDecorChance = 15;
    public void Start( ) {
        if (pGroundTileMap == null || pWaterShadowTileMap == null || pWaterTileMap == null || pGroundTiles.Length <= 0)
            return;

        this.GenerateMap( );
        this.transform.position -= new Vector3( nWidth / 2, nHeight / 2, 0 );
    }

    public void GenerateMap( ) {
        this.ClearMap( );

        this.pTerrainMap = new int[ nWidth, nHeight ];

        for (int i = 0 ; i < pTerrainMap.GetLength( 0 ) ; i++) {
            for (int j = 0 ; j < pTerrainMap.GetLength( 0 ) ; j++) {
                Vector3Int vNewTilePos = new Vector3Int( i, j, 0 );

                TileData_t pTileData = this.GenerateTileData( i, j, seed, octaves, lacunarity, persistance, scale );
                
                if (pTileData.nHeight <= 35) {
                    pGroundTileMap.SetTile( vNewTilePos, pGroundTiles[ Random.Range( 0, pGroundTiles.Length ) ] );
                    pWaterTileMap.SetTile( vNewTilePos, pWaterTiles[ Random.Range( 0, pWaterTiles.Length ) ] );
                    pWaterShadowTileMap.SetTile( vNewTilePos, pWaterShadowTile );
                } else {
                    pGroundTileMap.SetTile( vNewTilePos, pGroundTiles[ Random.Range( 0, pGroundTiles.Length ) ] );
                }
            }
        }
    }

    public TileData_t GenerateTileData( int x, int y, int seed, int octaves, float lacunarity, float persistance, float scale ) {
        TileData_t pTileData = new TileData_t( );

        System.Random rnd = new System.Random( seed );

        Vector2[ ] octOffsets = new Vector2[ octaves ];

        for (int i = 0 ; i < octaves ; i++) {
            float offsetX = rnd.Next( -25, 25 );
            float offsetY = rnd.Next( -25, 25 );
            octOffsets[ i ] = new Vector2( offsetX, offsetY );
        }

        float flFrequency = 1;
        float flHeight = 0;
        float flAmplitude = 1;
        float flTemperature = 0.0f;

        for (int i = 0 ; i < octaves ; i++) {
            float sampleX = x / scale * flFrequency + octOffsets[ i ].x;
            float sampleY = y / scale * flFrequency + octOffsets[ i ].y;

            flHeight += Mathf.PerlinNoise( sampleX, sampleY ) * flAmplitude;
            flTemperature += Mathf.PerlinNoise( sampleX, sampleY ) * flAmplitude;

            flAmplitude *= persistance;
            flFrequency *= lacunarity;
        }

        pTileData.nHeight = (int)(flHeight * 10);
        pTileData.nTemperature = (int)(flTemperature * 10);

        return pTileData;
    }

    public void ClearMap( ) {
        pGroundTileMap.ClearAllTiles( );
        pWaterTileMap.ClearAllTiles( );
    }
}