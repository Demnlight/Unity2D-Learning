
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class MapGeneratorHelper {

    public struct PerlinGeneratorSettings {
        public float flPerlinScale;
        public int flPerlinOctaves;
        public float flPersistence;
        public float flLacunarity;
        public float flPerlinBaseAmplitude;
        public Vector2[ ] vRandomOffsets;
    }

    public List<Vector2Int> aNearestChunksPos = new List<Vector2Int> { Vector2Int.zero };
    public Vector2Int vLastChunkPos = Vector2Int.one;

    public int Rounded( float flFrom ) {
        int nReturn = 0;
        if (flFrom > 0.0f)
            nReturn = (int)System.Math.Floor( flFrom );
        else
            nReturn = -(int)System.Math.Ceiling( System.Math.Abs( flFrom ) );

        return nReturn;
    }

    public bool PositionIsNew( Vector2 position ) {
        return (vLastChunkPos != position);
    }

    public void FillChunkHeights( Chunk_t pChunk, PerlinGeneratorSettings pSettings ) {
        for (int x = 0; x < MapGenerator.nChunkSize; x++) {
            for (int y = 0; y < MapGenerator.nChunkSize; y++) {
                float amplitude = pSettings.flPerlinBaseAmplitude;
                float freq = 1;
                float noiseHeight = 0;

                for (int i = 0; i < pSettings.flPerlinOctaves; i++) {
                    float px = (pChunk.vPos.x + x) / pSettings.flPerlinScale * freq + pSettings.vRandomOffsets[ i ].x;
                    float py = (pChunk.vPos.y + y) / pSettings.flPerlinScale * freq + pSettings.vRandomOffsets[ i ].y;

                    float PerlinValue = Mathf.PerlinNoise( px, py ) * 2 - 1;
                    noiseHeight += PerlinValue * amplitude;

                    amplitude *= pSettings.flPersistence;
                    freq *= pSettings.flLacunarity;
                }
                noiseHeight = Mathf.InverseLerp( -1f, 1f, noiseHeight );
                pChunk.Heights[ x, y ] = noiseHeight;
            }
        }
    }
    public Texture2D GenerateHeightMapTexture( Dictionary<Vector2Int, Chunk_t> aVisibleChunks, Vector2Int vStartChunk ) {
        Texture2D heightMap = new Texture2D( MapGenerator.nChunksSizeInLine, MapGenerator.nChunksSizeInLine );
        foreach (var element in aVisibleChunks) {
            for (int x = 0; x < MapGenerator.nChunkSize; x++) {
                for (int y = 0; y < MapGenerator.nChunkSize; y++) {
                    float height = element.Value.Heights[ x, y ];
                    Color colour = new Color( height, height, height, 1 );

                    int nStartPosX = System.Math.Abs( element.Value.vPos.x - aVisibleChunks[ vStartChunk ].vPos.x );
                    int nStartPosY = System.Math.Abs( element.Value.vPos.y - aVisibleChunks[ vStartChunk ].vPos.y );

                    heightMap.SetPixel( nStartPosX + x, nStartPosY + y, colour );
                }
            }
        }
        return heightMap;
    }
    public void SetupMaterialData( Material pWaterMaterial, Dictionary<Vector2Int, Chunk_t> aVisibleChunks, Texture2D pHeightMapTexture, Vector2Int vStartChunk ) {
        Vector2 vMapPos = new Vector2( aVisibleChunks[ vStartChunk ].vPos.x, aVisibleChunks[ vStartChunk ].vPos.y );

        pWaterMaterial.SetTexture( "_HeightMap", pHeightMapTexture );
        pWaterMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / MapGenerator.nChunksSizeInLine );
        pWaterMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );
    }

    public void FillNearestChunksUsingRenderDistance( int nRenderDistance ) {
        aNearestChunksPos.Clear( );

        for (int x = -nRenderDistance; x <= nRenderDistance; x++) {
            for (int y = -nRenderDistance; y <= nRenderDistance; y++) {
                aNearestChunksPos.Add( new Vector2Int( x, y ) );
            }
        }
    }

    public Vector2Int FindStartChunk( Dictionary<Vector2Int, Chunk_t> aVisibleChunks ) {
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
    public Vector2Int FindStartChunk( List<Vector2Int> aSearchebleZone ) {
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