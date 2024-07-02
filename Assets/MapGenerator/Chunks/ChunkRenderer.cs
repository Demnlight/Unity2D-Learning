using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Chunks {
    
    public interface IChunkRenderer {
        void GenerateHeightMapTexture( Dictionary<Vector2Int, Chunk> aRenderingChunks, Vector2Int vStartChunk );
        void SetupMaterials( Material[ ] materials, Vector2 vMapPos );
    }

    public class ChunkRenderer : IChunkRenderer {
        protected Texture2D HeightMapTexture = null;

        public void GenerateHeightMapTexture( Dictionary<Vector2Int, Chunk> aRenderingChunks, Vector2Int vStartChunk ) {
            HeightMapTexture = new Texture2D( ChunkConstants.nChunksSizeInLine, ChunkConstants.nChunksSizeInLine );

            foreach (var element in aRenderingChunks) {
                int nStartPosX = System.Math.Abs( element.Value.GetPosition.x - aRenderingChunks[ vStartChunk ].GetPosition.x );
                int nStartPosY = System.Math.Abs( element.Value.GetPosition.y - aRenderingChunks[ vStartChunk ].GetPosition.y );

                for (int x = 0; x < ChunkConstants.nChunkSize; x++) {
                    for (int y = 0; y < ChunkConstants.nChunkSize; y++) {
                        float height = element.Value.GetHeights[ x, y ];
                        Color colour = new Color( height, height, height, 1 );

                        HeightMapTexture.SetPixel( nStartPosX + x, nStartPosY + y, colour );
                    }
                }
            }

            HeightMapTexture.Apply( );
        }

        public void SetupMaterials( Material[ ] materials, Vector2 vMapPos ) {
            foreach (var material in materials) {
                material.SetTexture( "_HeightMap", HeightMapTexture );
                material.SetFloat( "_CurrentWorldTextureScale", 1.0f / ChunkConstants.nChunksSizeInLine );
                material.SetVector( "_CurrentWorldTexturePos", vMapPos );
            }
        }
    }
}

/*

    Tiles -> TileBase[chunksize * chunksize]; 
    area => current_chank.pos + vector(chunksize)
    TileMap.SetTilesBlock(Area, Array of tiles in area);



    Abstraction Level: 
    BaseChunk
        Chunk

    IChunkRenderer
        ChunkRenderer
            ChunkManager


    Tiles For fill right abstraction level =>
    
*/