using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class MapGeneratorHelper : MonoBehaviour {

    private static float PerlinScale = 48.0f;
    private static int PerlinOctaves = 5;
    private static float persistence = 1f;
    private static float lacunarity = 1f;
    private static float PerlinBaseAmplitude = 0.52f;
    public static List<Vector2Int> aNearestChunksPos = new List<Vector2Int> {
            Vector2Int.zero
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
        for (int x = 0; x < MapGenerator.nChunkSize; x++) {
            for (int y = 0; y < MapGenerator.nChunkSize; y++) {
                float amplitude = PerlinBaseAmplitude;
                float freq = 1;
                float noiseHeight = 0;

                for (int i = 0; i < PerlinOctaves; i++) {
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
    public static Texture2D GenerateHeightMapTexture( Dictionary<Vector2Int, Chunk_t> aVisibleChunks ) {
        Texture2D heightMap = new Texture2D( MapGenerator.nChunksSizeInLine, MapGenerator.nChunksSizeInLine );
        foreach (var element in aVisibleChunks) {
            for (int x = 0; x < MapGenerator.nChunkSize; x++) {
                for (int y = 0; y < MapGenerator.nChunkSize; y++) {
                    float height = element.Value.Heights[ x, y ];
                    Color colour = new Color( height, height, height, 1 );

                    int nStartPosX = Math.Abs( element.Value.vPos.x - aVisibleChunks[ MapGenerator.vStartChunk ].vPos.x );
                    int nStartPosY = Math.Abs( element.Value.vPos.y - aVisibleChunks[ MapGenerator.vStartChunk ].vPos.y );

                    heightMap.SetPixel( nStartPosX + x, nStartPosY + y, colour );
                }
            }
        }
        return heightMap;
    }
    public static void SetupMaterialData( Material pWaterMaterial, Dictionary<Vector2Int, Chunk_t> aVisibleChunks, Texture2D pHeightMapTexture ) {
        Vector2 vMapPos = new Vector2( aVisibleChunks[ MapGenerator.vStartChunk ].vPos.x, aVisibleChunks[ MapGenerator.vStartChunk ].vPos.y );

        pWaterMaterial.SetTexture( "_HeightMap", pHeightMapTexture );
        pWaterMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / MapGenerator.nChunksSizeInLine );
        pWaterMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );
    }

    public static void FillNearestChunksUsingRenderDistance( int nRenderDistance ) {
        aNearestChunksPos.Clear( );

        for (int x = -nRenderDistance; x <= nRenderDistance; x++) {
            for (int y = -nRenderDistance; y <= nRenderDistance; y++) {
                aNearestChunksPos.Add( new Vector2Int( x, y ) );
            }
        }
    }

    public static Vector2Int FindStartChunk( Dictionary<Vector2Int, Chunk_t> aVisibleChunks ) {
        Vector2Int vReturn = Vector2Int.zero;

        int nMinX = int.MaxValue;
        int nMinY = int.MaxValue;

        foreach (var pChunk in aVisibleChunks) {
            if (pChunk.Value.vPos.x < nMinX)
                nMinX = pChunk.Value.vPos.x;
            if (pChunk.Value.vPos.y < nMinY)
                nMinY = pChunk.Value.vPos.y;

        }
        vReturn = new Vector2Int( nMinX, nMinY );
        return vReturn;
    }
    public static Vector2Int FindStartChunk( List<Vector2Int> aSearchebleZone ) {
        Vector2Int vReturn = Vector2Int.zero;

        int nMinX = int.MaxValue;
        int nMinY = int.MaxValue;

        foreach (Vector2Int vCoords in aSearchebleZone) {
            if (vCoords.x < nMinX)
                nMinX = vCoords.x;
            if (vCoords.y < nMinY)
                nMinY = vCoords.y;

        }
        vReturn = new Vector2Int( nMinX, nMinY );
        return vReturn;
    }
}