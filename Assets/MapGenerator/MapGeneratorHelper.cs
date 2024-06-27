using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneratorHelper : MonoBehaviour {

    private static float PerlinScale = 48.0f;
    private static int PerlinOctaves = 5;
    private static float persistence = 1f;
    private static float lacunarity = 1f;
    private static float PerlinBaseAmplitude = 0.52f;
    public static List<Vector2Int> aNearestChunksPos = new List<Vector2Int> {
            new Vector2Int( -1, 1 ),
            Vector2Int.up,
            Vector2Int.one,
            Vector2Int.left,
            Vector2Int.zero,
            Vector2Int.right,
            -Vector2Int.one,
            Vector2Int.down,
            new Vector2Int( 1, -1 )
        };
    public static Vector2Int vLastChunkPos = Vector2Int.one;


    public static int Rounded( float flFrom ) {
        int nReturn = 0;

        if (flFrom > 0.0f) {
            nReturn = (int)Math.Floor( flFrom );
        } else {
            nReturn = -(int)Math.Ceiling( Math.Abs( flFrom ) );
        }

        return nReturn;
    }

    public static bool PositionIsNew( Vector2 position ) {
        return (vLastChunkPos != position);
    }

    public static void FillChunkHeights( Chunk_t pChunk ) {
        for (int x = 0 ; x < MapGenetator.ChunkSize ; x++) {
            for (int y = 0 ; y < MapGenetator.ChunkSize ; y++) {
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
    public static Texture2D GenerateHeightMapTexture( List<Chunk_t> aVisibleChunks ) {
        Texture2D heightMap = new Texture2D( MapGenetator.ChunkSize * 3, MapGenetator.ChunkSize * 3 );
        foreach (Chunk_t pChunk in aVisibleChunks) {
            for (int x = 0 ; x < MapGenetator.ChunkSize ; x++) {
                for (int y = 0 ; y < MapGenetator.ChunkSize ; y++) {
                    float height = pChunk.Heights[ x, y ];
                    Color colour = new Color( height, height, height, 1 );

                    int nStartPosX = Math.Abs( pChunk.vPos.x - aVisibleChunks[ 0 ].vPos.x );
                    int nStartPosY = Math.Abs( pChunk.vPos.y - aVisibleChunks[ 6 ].vPos.y );

                    heightMap.SetPixel( nStartPosX + x, nStartPosY + y, colour );
                }
            }
        }
        return heightMap;
    }
    public static void SetupMaterialData( Material pWaterMaterial, List<Chunk_t> aVisibleChunks, Texture2D pHeightMapTexture ) {
        Vector2 vMapPos = new Vector2( aVisibleChunks[ 6 ].vPos.x, aVisibleChunks[ 6 ].vPos.y );

        pWaterMaterial.SetTexture( "_HeightMap", pHeightMapTexture );
        pWaterMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / (MapGenetator.ChunkSize * 3) );
        pWaterMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );
    }
}