
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.MapGenerator {
    public class ChunkConsts {
        public const int nChunkSize = 16;
        public const int nRenderDistance = 1;
        public const int nRenderDistanceSize = nRenderDistance * nChunkSize;
        public const int nChunksCountInLine = nRenderDistance * 2 + 1;
        public const int nChunksSizeInLine = nChunksCountInLine * nChunkSize;
    }
    public class ChunksData {
        public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
        public Dictionary<Vector2Int, Chunk_t> aVisibleChunks = new Dictionary<Vector2Int, Chunk_t>( );
    }

    public struct Chunk_t {
        public Vector2Int vPos;
        public float[ , ] Heights;
        public bool bVisible;
    }
}