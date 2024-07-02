using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Scripts.Chunks {

    public struct TileData {
        public Tilemap pTileMap;
        public TileBase pTileForFill;
        public float flMinHeight;
        public float flMaxHeight;
    }

    public struct ChunkData {
        public Vector2Int vPos;
        public float[ , ] aHeights;
        public bool bRendering;
    }

    public interface IChunk {
        void GenerateHeightMap( );
        void FillTileMap( TileData tileData );
    }

    public abstract class BaseChunk : IChunk {
        protected ChunkData data;
        public abstract void GenerateHeightMap( );
        public abstract void FillTileMap( TileData tileData );
    }

    public class Chunk : BaseChunk {

        public Chunk( ChunkData data ) => this.data = data;

        public Vector2Int GetPosition { get => data.vPos; }
        public float[ , ] GetHeights { get => data.aHeights; }
        public bool IsRendering { get => data.bRendering; }

        public void Rendering( ) => this.data.bRendering = true;
        public void NotRendering( ) => this.data.bRendering = false;

        public override void GenerateHeightMap( ) {
            this.data.aHeights = new float[ ChunkConstants.nChunkSize, ChunkConstants.nChunkSize ];

            for (int x = 0; x < ChunkConstants.nChunkSize; x++)
                for (int y = 0; y < ChunkConstants.nChunkSize; y++)
                    this.data.aHeights[ x, y ] = Initialization.perlinGenerator.GetHeight( this.data.vPos, new Vector2Int( x, y ) );
        }

        public override void FillTileMap( TileData tileData ) {
            TileBase[ ] aTilesBlock = new TileBase[ ChunkConstants.nChunkSize * ChunkConstants.nChunkSize ];

            int i = 0;
            for (int y = 0; y < ChunkConstants.nChunkSize; y++) {
                for (int x = 0; x < ChunkConstants.nChunkSize; x++) {
                    float flHeight = this.data.aHeights[ x, y ];

                    if (flHeight >= tileData.flMinHeight && flHeight <= tileData.flMaxHeight)
                        aTilesBlock[ i ] = tileData.pTileForFill;

                    i++;
                }
            }

            BoundsInt FillArea = new BoundsInt(
                this.data.vPos.x, this.data.vPos.y, 0,
                ChunkConstants.nChunkSize, ChunkConstants.nChunkSize, 1
            );

            tileData.pTileMap.SetTilesBlock( FillArea, aTilesBlock );
        }

        public bool IsFar( Vector2Int vFrom, int nMaxDistance ) {
            int nXDistance = Math.Abs( this.data.vPos.x - vFrom.x );
            int nYDistance = Math.Abs( this.data.vPos.y - vFrom.y );
            bool bXOutOfDistance = nXDistance > nMaxDistance;
            bool bYOutOfDistance = nYDistance > nMaxDistance;

            return bXOutOfDistance || bYOutOfDistance;
        }
    }
}