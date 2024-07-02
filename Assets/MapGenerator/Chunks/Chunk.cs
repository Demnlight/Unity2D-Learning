using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.Chunks {

    public abstract class AbstractChunk {
        public Vector2Int vPos;
        public abstract Vector2Int Position { get; set; }
        public float[ , ] aMapHeights;
        public bool bRendering = false;
        public abstract float[ , ] GetMapHeights { get; }
        public abstract void GenerateHeightMap( );
        public abstract TileBase[ ] GetTiles( float flMinTileHeight, float flMaxTileHeight, TileBase pTile );
    }

    public class Chunk : AbstractChunk {
        public override Vector2Int Position { get => vPos; set => vPos = value; }
        public override float[ , ] GetMapHeights { get => aMapHeights; }

        public override void GenerateHeightMap( ) {
            this.aMapHeights = new float[ ChunkConstants.nChunkSize, ChunkConstants.nChunkSize ];

            for (int x = 0; x < ChunkConstants.nChunkSize; x++)
                for (int y = 0; y < ChunkConstants.nChunkSize; y++)
                    this.aMapHeights[ x, y ] = Initialization.perlinGenerator.GetHeight( this.vPos, new Vector2Int( x, y ) );
        }

        public override TileBase[ ] GetTiles( float flMinTileHeight, float flMaxTileHeight, TileBase pTile ) {
            TileBase[ ] pTiles = new TileBase[ ChunkConstants.nChunkSize * ChunkConstants.nChunkSize ];

            int i = 0;
            for (int y = 0; y < ChunkConstants.nChunkSize; y++) {
                for (int x = 0; x < ChunkConstants.nChunkSize; x++) {

                    float flHeight = this.aMapHeights[ x, y ];
                    if (flHeight >= flMinTileHeight && flHeight <= flMaxTileHeight)
                        pTiles[ i ] = pTile;
                    i++;
                }
            }

            return pTiles;
        }

        public bool IsFar( Vector2Int vFrom, int nMaxDistance ) {
            int nXDistance = Math.Abs( this.vPos.x - vFrom.x );
            int nYDistance = Math.Abs( this.vPos.y - vFrom.y );
            bool bXOutOfDistance = nXDistance > nMaxDistance;
            bool bYOutOfDistance = nYDistance > nMaxDistance;

            return bXOutOfDistance || bYOutOfDistance;
        }
    }
}