using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class MapGenetator : MonoBehaviour {
    public int nWidth = 0;
    public int nHeight = 0;
    public Tilemap pTileMap = null;

    public Tile[ ] pGroundTiles = null;
    public Tile[ ] pWallTiles = null;
    public Tile[ ] pWaterTiles = null;

    public int[ , ] pTerrainMap;

    public void Start( ) {
        if (pTileMap == null || pGroundTiles.Length <= 0 || pWallTiles.Length <= 0 || pWaterTiles.Length <= 0)
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

                int nTileType = this.GetTile( i, j, 10 );

                //pWaterTiles[ 0 ].color = new Color( nTileType, nTileType, nTileType, 255 );
                pGroundTiles[ 0 ].color = new Color( nTileType, nTileType, nTileType, 255 );
                //pWallTiles[ 0 ].color = new Color( nTileType, nTileType, nTileType, 255 );

                pTileMap.SetTile( vNewTilePos, pGroundTiles[ 0 ] );

            }
        }
    }

    public int GetTile( int x, int y, float seed ) {
        int nReturnIndex = -1;

        float sampleX = x / seed;
        float sampleZ = y / seed;

        nReturnIndex = (int)(Mathf.PerlinNoise( sampleX, sampleZ ) * 100);

        return nReturnIndex;
    }

    public void ClearMap( ) {
        pTileMap.ClearAllTiles( );
    }


}