

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.Chunks {
    using AdditionalMath;

    public struct TileSetupperSettings {
        public TileBase[ ] pTiles;
        public Tilemap[ ] pMaps;
        public float[ ] pMinTilesHeights;
        public float[ ] pMaxTilesHeights;
        public Material[ ] aMaterials;
    }

    public interface IChunkManager {
        Vector2Int ConvertGlobalPosToLocal( Vector2 vGlobalPos );

        Chunk GetChunk( int x, int y );
        Chunk GetChunk( Vector2Int vChunkPos );
        Vector2Int GetStartChunk( );

        void GenerateChunk( Vector2Int vChunkPos );
        void GenerateChunk( int x, int y );
        void GenerateChunksAround( Vector2Int vStartPos, int nGenerateDistance );

        void ClearAllRenderingChunks( );
        void ClearAllCachedChunks( );
        void ClearChunks( );

        List<Vector2Int> GetClearedFarChunks( Dictionary<Vector2Int, Chunk> pChunks, Vector2Int vStartPos, int nMaxDistance );
        void ClearTileMaps( List<Vector2Int> vRemovedChunksCoord );
    }

    public class ChunkManager : ChunkRenderer, IChunkManager {
        private ChunksHelper chunksHelper = new ChunksHelper( );

        private Vector2Int vStartChunk;

        private TileSetupperSettings tileSetupperSettings = new TileSetupperSettings( );

        private Dictionary<Vector2Int, Chunk> aCachedChunks = new Dictionary<Vector2Int, Chunk>( );
        private Dictionary<Vector2Int, Chunk> aRenderingChunks = new Dictionary<Vector2Int, Chunk>( );

        public ChunkManager( TileSetupperSettings settings ) {
            this.tileSetupperSettings = settings;
        }

        public ChunkManager( TileBase[ ] pTiles, Tilemap[ ] pMaps, float[ ] pMinTilesHeights, float[ ] pMaxTilesHeights, Material[ ] aMaterials ) {
            this.tileSetupperSettings.pTiles = pTiles;
            this.tileSetupperSettings.pMaps = pMaps;
            this.tileSetupperSettings.pMinTilesHeights = pMinTilesHeights;
            this.tileSetupperSettings.pMaxTilesHeights = pMaxTilesHeights;
            this.tileSetupperSettings.aMaterials = aMaterials;
        }

        public void GenerateChunksAround( Vector2Int vStartPos, int nGenerateDistance ) {
            List<Vector2Int> aScaledChunksCoord = chunksHelper.GetChunksAround( nGenerateDistance );

            foreach (Vector2Int vChunkOffset in aScaledChunksCoord) {
                Vector2Int nChunkStartPos = new Vector2Int(
                    vStartPos.x + vChunkOffset.x * ChunkConstants.nChunkSize,
                    vStartPos.y + vChunkOffset.y * ChunkConstants.nChunkSize );

                Chunk offsetChunk = GetChunk( nChunkStartPos );
                if (!offsetChunk.IsRendering) {
                    offsetChunk.Rendering( );

                    for (int layer = 0; layer < tileSetupperSettings.pMaps.Length; layer++) {
                        TileData tileData = new TileData {
                            pTileMap = tileSetupperSettings.pMaps[ layer ],
                            pTileForFill = tileSetupperSettings.pTiles[ layer ],
                            flMinHeight = tileSetupperSettings.pMinTilesHeights[ layer ],
                            flMaxHeight = tileSetupperSettings.pMaxTilesHeights[ layer ]
                        };

                        offsetChunk.FillTileMap( tileData );
                    }

                    aRenderingChunks.Add( offsetChunk.GetPosition, offsetChunk );
                    //Debug.LogFormat( "Generated: {0}", offsetChunk.GetPosition );
                }
            }

            List<Vector2Int> aClearedPositions = this.GetClearedFarChunks( this.aRenderingChunks, vStartPos, ChunkConstants.nRenderDistanceScaled );
            this.GetClearedFarChunks( this.aCachedChunks, vStartPos, ChunkConstants.nRenderDistanceScaled * 2 );
            this.ClearTileMaps( aClearedPositions );

            vStartChunk = this.GetStartChunk( );

            base.GenerateHeightMapTexture( this.aRenderingChunks, vStartChunk );
            base.SetupMaterials( tileSetupperSettings.aMaterials, vStartChunk );
        }

        public Vector2Int ConvertGlobalPosToLocal( Vector2 vGlobalPos ) {
            Vector2Int vLocalPos = new Vector2Int( );
            vLocalPos.x = AdditionalMath.RoundFrom( vGlobalPos.x / ChunkConstants.nChunkSize ) * ChunkConstants.nChunkSize;
            vLocalPos.y = AdditionalMath.RoundFrom( vGlobalPos.y / ChunkConstants.nChunkSize ) * ChunkConstants.nChunkSize;
            return vLocalPos;
        }

        public Vector2Int ConvertGlobalPosToLocalScaled( Vector2 vGlobalPos ) {
            Vector2Int vLocalPos = new Vector2Int( );
            vLocalPos.x = AdditionalMath.RoundFrom( vGlobalPos.x / ChunkConstants.nChunkSize );
            vLocalPos.y = AdditionalMath.RoundFrom( vGlobalPos.y / ChunkConstants.nChunkSize );
            return vLocalPos;
        }

        public void GenerateChunk( Vector2Int vChunkPos ) {

            ChunkData data = new ChunkData {
                vPos = vChunkPos,
                bRendering = false
            };

            Chunk NewChunk = new Chunk( data );
            NewChunk.GenerateHeightMap( );
            this.aCachedChunks.Add( vChunkPos, NewChunk );
        }

        public void GenerateChunk( int x, int y ) =>
            this.GenerateChunk( new Vector2Int( x * ChunkConstants.nChunkSize, y * ChunkConstants.nChunkSize ) );

        public Vector2Int GetStartChunk( ) {
            Vector2Int vReturn = Vector2Int.zero;

            int nMinX = int.MaxValue;
            int nMinY = int.MaxValue;

            foreach (var pChunk in this.aRenderingChunks) {
                if (pChunk.Value.GetPosition.x < nMinX)
                    nMinX = pChunk.Value.GetPosition.x;
                if (pChunk.Value.GetPosition.y < nMinY)
                    nMinY = pChunk.Value.GetPosition.y;

            }
            vReturn = new Vector2Int( nMinX, nMinY );
            return vReturn;
        }

        public void ClearAllRenderingChunks( ) =>
            this.aRenderingChunks.Clear( );

        public void ClearAllCachedChunks( ) =>
            this.aCachedChunks.Clear( );

        public void ClearChunks( ) {
            this.ClearAllCachedChunks( );
            this.ClearAllRenderingChunks( );
        }

        public List<Vector2Int> GetClearedFarChunks( Dictionary<Vector2Int, Chunk> pChunks, Vector2Int vStartPos, int nMaxDistance ) {
            List<Vector2Int> vToRemoveCoords = new List<Vector2Int>( );
            foreach (Chunk pChunk in pChunks.Values) {
                if (pChunk.IsFar( vStartPos, nMaxDistance ))
                    vToRemoveCoords.Add( pChunk.GetPosition );
            }

            foreach (Vector2Int vCoords in vToRemoveCoords) {
                pChunks.Remove( vCoords );
                //Debug.LogFormat( "Removed: {0}", vCoords );
            }

            return vToRemoveCoords;
        }

        public void ClearTileMaps( List<Vector2Int> vRemovedChunksCoord ) {
            foreach (var v in vRemovedChunksCoord) {
                BoundsInt bounds = new BoundsInt(
                    v.x, v.y, 0,
                    ChunkConstants.nChunkSize, ChunkConstants.nChunkSize, 1
                );

                TileBase[ ] nullTiles = new TileBase[ ChunkConstants.nChunkSize * ChunkConstants.nChunkSize ];
                for (int i = 0; i < this.tileSetupperSettings.pMaps.Length; i++)
                    this.tileSetupperSettings.pMaps[ i ].SetTilesBlock( bounds, nullTiles );
            }
        }

        public Dictionary<Vector2Int, Chunk> GetRenderingChunks( ) => this.aRenderingChunks;
        public Dictionary<Vector2Int, Chunk> GetCachedChunks( ) => this.aCachedChunks;

        public Chunk GetChunk( int x, int y ) =>
                    this.GetChunk( new Vector2Int( x * ChunkConstants.nChunkSize, y * ChunkConstants.nChunkSize ) );

        public Chunk GetChunk( Vector2Int vChunkPos ) {
            if (this.aRenderingChunks.ContainsKey( vChunkPos )) {
                return this.aRenderingChunks[ vChunkPos ];
            } else {
                if (!this.aCachedChunks.ContainsKey( vChunkPos ))
                    GenerateChunk( vChunkPos );

                this.aCachedChunks[ vChunkPos ].NotRendering( );
                return this.aCachedChunks[ vChunkPos ];
            }
        }
    }
}