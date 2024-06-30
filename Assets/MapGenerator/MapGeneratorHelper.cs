
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Scripts.MapGenerator {
    public class Helper {
        public int Rounded( float flFrom ) {
            int nReturn = 0;
            if (flFrom > 0.0f)
                nReturn = (int)System.Math.Floor( flFrom );
            else
                nReturn = -(int)System.Math.Ceiling( System.Math.Abs( flFrom ) );

            return nReturn;
        }

        public Texture2D GenerateHeightMapTexture( Dictionary<Vector2Int, Chunk_t> aVisibleChunks, Vector2Int vStartChunk ) {
            Texture2D heightMap = new Texture2D( ChunkConsts.nChunksSizeInLine, ChunkConsts.nChunksSizeInLine );
            foreach (var element in aVisibleChunks) {
                for (int x = 0; x < ChunkConsts.nChunkSize; x++) {
                    for (int y = 0; y < ChunkConsts.nChunkSize; y++) {
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

        public void SetupMaterialData( Material pMaterial, Dictionary<Vector2Int, Chunk_t> aVisibleChunks, Texture2D pHeightMapTexture, Vector2Int vStartChunk ) {
            Vector2 vMapPos = new Vector2( aVisibleChunks[ vStartChunk ].vPos.x, aVisibleChunks[ vStartChunk ].vPos.y );

            pMaterial.SetTexture( "_HeightMap", pHeightMapTexture );
            pMaterial.SetFloat( "_CurrentWorldTextureScale", 1.0f / ChunkConsts.nChunksSizeInLine );
            pMaterial.SetVector( "_CurrentWorldTexturePos", vMapPos );
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

        public TileBase[ ] GetTiles( Chunk_t pChunk, float flMinTileHeight, float flMaxTileHeight, TileBase pTile ) {
            TileBase[ ] pTiles = new TileBase[ ChunkConsts.nChunkSize * ChunkConsts.nChunkSize ];

            int i = 0;
            for (int y = 0; y < ChunkConsts.nChunkSize; y++) {
                for (int x = 0; x < ChunkConsts.nChunkSize; x++) {

                    float flHeight = pChunk.Heights[ x, y ];
                    if (flHeight >= flMinTileHeight && flHeight <= flMaxTileHeight)
                        pTiles[ i ] = pTile;
                    i++;
                }
            }

            return pTiles;
        }
    }
}