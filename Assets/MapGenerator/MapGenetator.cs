using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEditor;

public struct Chunk_t {
    public Vector2Int vPos;
    public float[ , ] Heights;
    public bool bVisible;
}

public class MapGenetator : MonoBehaviour {
    public int ChunkSize = 16;
    public string Seed = "";
    public float PerlinScale = 48.0f;
    public int PerlinOctaves = 5;
    public float persistence = 1f;
    public float lacunarity = 1f;
    public float PerlinBaseAmplitude = 0.52f;
    public Dictionary<Vector2Int, Chunk_t> aAllChunks = new Dictionary<Vector2Int, Chunk_t>( );
    public List<Chunk_t> aVisibleChunks = new List<Chunk_t>( );

    public BasePlayer pPlayer = null;

    private Vector2Int vLastChunkPos = Vector2Int.zero;
    private void Awake( ) {
        if (!pPlayer)
            return;
        aAllChunks.Clear( );
        aVisibleChunks.Clear( );
        List<Vector2Int> aNearestChunksPos = new List<Vector2Int> {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.one,
            -Vector2Int.one,
            new Vector2Int( 1, -1 ),
            new Vector2Int( -1, 1 ),
            Vector2Int.zero
        };

        foreach (Vector2Int vChunkOffset in aNearestChunksPos) {
            Vector2Int vRealChunkPos = vChunkOffset * ChunkSize;
            GenerateChunk( vRealChunkPos.x, vRealChunkPos.y );
            Chunk_t pPreviewChank = GetChunk( vRealChunkPos.x, vRealChunkPos.y );
            pPreviewChank.bVisible = true;
            this.aVisibleChunks.Add( pPreviewChank );
        }
    }

    private void Update( ) {
        Vector2Int chunkPos = new Vector2Int( 0, 0 );

        if (Camera.main.transform.position.x >= 0)
            chunkPos.x = (int)((Camera.main.transform.position.x + ChunkSize) / ChunkSize);
        else
            chunkPos.x = (int)((Camera.main.transform.position.x - ChunkSize) / ChunkSize);

        if (Camera.main.transform.position.y >= 0)
            chunkPos.y = (int)((Camera.main.transform.position.y + ChunkSize) / ChunkSize);
        else
            chunkPos.y = (int)((Camera.main.transform.position.y - ChunkSize) / ChunkSize);

        EnableChunks( chunkPos );
    }

    private void EnableChunks( Vector2Int vChunkPos ) {
        Vector2Int vNewChunkPos = new Vector2Int( vChunkPos.x * ChunkSize, vChunkPos.y * ChunkSize );
        if (!PositionIsNew( vNewChunkPos ))
            return;

        this.aVisibleChunks.Clear( );

        List<Vector2Int> aNearestChunksPos = new List<Vector2Int> {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.one,
            -Vector2Int.one,
            new Vector2Int( 1, -1 ),
            new Vector2Int( -1, 1 ),
            Vector2Int.zero
        };

        foreach (Vector2Int vChunkOffset in aNearestChunksPos) {
            Chunk_t offsetChunk = GetChunk(
                vNewChunkPos.x + vChunkOffset.x * ChunkSize,
                vNewChunkPos.y + vChunkOffset.y * ChunkSize );

            offsetChunk.bVisible = true;
            this.aVisibleChunks.Add( offsetChunk );
        }
        vLastChunkPos = vNewChunkPos;
    }

    public bool PositionIsNew( Vector2Int position ) {
        return (vLastChunkPos != position);
    }

    public void GenerateChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (!this.aAllChunks.ContainsKey( vChunkPos )) {
            Chunk_t pNewChunk = new Chunk_t( );
            pNewChunk.vPos = vChunkPos;
            pNewChunk.Heights = new float[ ChunkSize, ChunkSize ];

            this.aAllChunks.Add( vChunkPos, pNewChunk );
            GenerateChunkData( this.aAllChunks[ vChunkPos ] );
        }
    }
    public Chunk_t GetChunk( int x, int y ) {
        Vector2Int vChunkPos = new Vector2Int( x, y );
        if (this.aAllChunks.ContainsKey( vChunkPos )) {
            return this.aAllChunks[ vChunkPos ];
        } else {
            GenerateChunk( x, y );
            return this.aAllChunks[ vChunkPos ];
        }
    }

    private void GenerateChunkData( Chunk_t pChunk ) {
        for (int x = 0 ; x < ChunkSize ; x++) {
            for (int y = 0 ; y < ChunkSize ; y++) {
                float amplitude = PerlinBaseAmplitude;
                float freq = 1;
                float noiseHeight = 0;

                for (int i = 0 ; i < PerlinOctaves ; i++) {
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

    private void OnDrawGizmos( ) {
        if (!EditorApplication.isPlaying) {
            Awake( );
        }
        foreach (Chunk_t c in this.aVisibleChunks) {
            Vector3 ChunkPosition = new Vector3( c.vPos.x * ChunkSize, c.vPos.y * ChunkSize );
            for (int x = 0 ; x < ChunkSize ; x++) {
                for (int y = 0 ; y < ChunkSize ; y++) {
                    Vector3 vCellPos = new Vector3( c.vPos.x + x + 0.5f, c.vPos.y + y + 0.5f, 0 );

                    float flHeight = c.Heights[ x, y ];
                    Gizmos.color = new Color( flHeight, flHeight, flHeight, 1 );
                    Gizmos.DrawCube( vCellPos, Vector3.one );
                }
            }
        }
    }
}