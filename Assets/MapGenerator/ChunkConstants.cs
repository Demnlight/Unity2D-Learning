using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkConstants {
    public const int nChunkSize = 16;
    public const int nRenderDistance = 1;
    public const int nRenderDistanceScaled = nRenderDistance * nChunkSize;
    public const int nChunksCountInLine = nRenderDistance * 2 + 1;
    public const int nChunksSizeInLine = nChunksCountInLine * nChunkSize;
}