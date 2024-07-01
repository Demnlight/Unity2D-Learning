
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.MapGenerator {
    public struct PerlinGeneratorSettings {
        public float flPerlinScale;
        public int nPerlinOctaves;
        public float flPersistence;
        public float flLacunarity;
        public float flPerlinBaseAmplitude;
        public Vector2[ ] vRandomOffsets;
    }

    public class ChunkConsts {
        public const int nChunkSize = 16;
        public const int nRenderDistance = 1;
        public const int nRenderDistanceSize = nRenderDistance * nChunkSize;
        public const int nChunksCountInLine = nRenderDistance * 2 + 1;
        public const int nChunksSizeInLine = nChunksCountInLine * nChunkSize;

        public readonly Dictionary<ChunksBiomes_t, Biome_t> aBiomes = new Dictionary<ChunksBiomes_t, Biome_t>( ) {
            { ChunksBiomes_t.RIVER, new Biome_t( 0.4f, 0.5f, 0, 50, 50, 100 ) },
            { ChunksBiomes_t.OCEAN, new Biome_t( 0f, 0.4f, 0, 50, 50, 100 ) },
            { ChunksBiomes_t.BEACH, new Biome_t( 0.5f, 0.55f, 0, 50, 50, 100 ) },
            { ChunksBiomes_t.FOREST, new Biome_t( 0.5f, 0.75f, 0, 30, 40, 80 ) },
            { ChunksBiomes_t.NONE, new Biome_t( 0f, 1.0f, 0, 100, 0, 100 ) },
        };
    }

    public class ChunksData {
        public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
        public Dictionary<Vector2Int, Chunk_t> aVisibleChunks = new Dictionary<Vector2Int, Chunk_t>( );
        public List<Vector2Int> aNearestChunksPos = new List<Vector2Int> { Vector2Int.zero };
        public Vector2Int vLastChunkPos = Vector2Int.one;
        public PerlinGeneratorSettings pSettings = new PerlinGeneratorSettings( );

        public Chunk_t GetChunk( int x, int y ) {
            Vector2Int vChunkPos = new Vector2Int( x, y );
            if (this.aVisibleChunks.ContainsKey( vChunkPos )) {
                return this.aVisibleChunks[ vChunkPos ];
            } else {
                if (!this.aAllChunks.ContainsKey( vChunkPos ))
                    GenerateChunk( x, y );

                return this.aAllChunks[ vChunkPos ];
            }
        }

        public void GenerateChunk( int x, int y ) {
            Vector2Int vChunkPos = new Vector2Int( x, y );
            if (!this.aAllChunks.ContainsKey( vChunkPos )) {
                Chunk_t pNewChunk = new Chunk_t( );

                pNewChunk.vPos = vChunkPos;
                pNewChunk.Heights = new float[ ChunkConsts.nChunkSize, ChunkConsts.nChunkSize ];
                this.FillChunkHeights( pNewChunk, pSettings );

                this.aAllChunks.Add( vChunkPos, pNewChunk );
            }
        }

        public List<Chunk_t> GenerateVisibleChunks( Vector2Int vNewChunkPos ) {
            List<Chunk_t> vAddedChunks = new List<Chunk_t>( );
            foreach (Vector2Int vChunkOffset in this.aNearestChunksPos) {
                Vector2Int nChunkStartPos = new Vector2Int(
                    vNewChunkPos.x + vChunkOffset.x * ChunkConsts.nChunkSize,
                    vNewChunkPos.y + vChunkOffset.y * ChunkConsts.nChunkSize );

                Chunk_t offsetChunk = this.GetChunk( nChunkStartPos.x, nChunkStartPos.y );
                if (!offsetChunk.bVisible) {
                    offsetChunk.bVisible = true;
                    vAddedChunks.Add( offsetChunk );
                    this.aVisibleChunks.Add( offsetChunk.vPos, offsetChunk );
                }
            }

            return vAddedChunks;
        }

        public void FillChunkHeights( Chunk_t pChunk, PerlinGeneratorSettings pSettings ) {
            for (int x = 0; x < ChunkConsts.nChunkSize; x++) {
                for (int y = 0; y < ChunkConsts.nChunkSize; y++) {
                    float amplitude = pSettings.flPerlinBaseAmplitude;
                    float freq = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < pSettings.nPerlinOctaves; i++) {
                        float px = (pChunk.vPos.x + x) / pSettings.flPerlinScale * freq + pSettings.vRandomOffsets[ i ].x;
                        float py = (pChunk.vPos.y + y) / pSettings.flPerlinScale * freq + pSettings.vRandomOffsets[ i ].y;

                        float PerlinValue = Mathf.PerlinNoise( px, py ) * 2 - 1;
                        noiseHeight += PerlinValue * amplitude;

                        amplitude *= pSettings.flPersistence;
                        freq *= pSettings.flLacunarity;
                    }
                    noiseHeight = Mathf.InverseLerp( -1f, 1f, noiseHeight );

                    noiseHeight = RoundUp( noiseHeight, 2 );

                    pChunk.Heights[ x, y ] = noiseHeight;
                }
            }
        }

        public void GetNearestChunks( int nRenderDistance ) {
            aNearestChunksPos.Clear( );

            for (int x = -nRenderDistance; x <= nRenderDistance; x++) {
                for (int y = -nRenderDistance; y <= nRenderDistance; y++) {
                    aNearestChunksPos.Add( new Vector2Int( x, y ) );
                }
            }
        }

        public bool PositionIsNew( Vector2 position ) {
            return (vLastChunkPos != position);
        }

        public static float RoundUp( float number, int digits ) {
            var factor = Convert.ToDouble( Math.Pow( 10, digits ) );
            var dnumber = Convert.ToDouble( number );
            return (float)(Math.Ceiling( dnumber * factor ) / factor);
        }
    }

    public struct Chunk_t {
        public Vector2Int vPos;
        public float[ , ] Heights;
        public bool bVisible;
    }

    public struct Biome_t {
        float flMinHeight;
        float flMaxHeight;
        int nMinTemperature;
        int nMaxTemperature;
        float flMinHumidity;
        float flMaxHumidity;

        public Biome_t(
            float flMinHeight, float flMaxHeight,
            int nMinTemperature, int nMaxTemperature,
            float flMinHumidity, float flMaxHumidity ) {
            this.flMinHeight = flMinHeight;
            this.flMaxHeight = flMaxHeight;
            this.nMinTemperature = nMinTemperature;
            this.nMaxTemperature = nMaxTemperature;
            this.flMinHumidity = flMinHumidity;
            this.flMaxHumidity = flMaxHumidity;
        }
    }
    public enum ChunksBiomes_t {
        NONE,
        OCEAN,
        RIVER,
        BEACH,
        FOREST
    }
}