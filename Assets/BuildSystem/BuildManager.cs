using System;
using Scripts.AdditionalMath;
using Scripts.Build;
using Scripts.Chunks;
using UnityEngine;

public class BuildManager : MonoBehaviour {
    [SerializeField] private MapGenerator mapGenerator = null;
    [SerializeField] private Camera camera = null;

    private IChunkManager chunkManager = null;
    private CursorManager cursorManager = null;
    private Vector2Int vCursorWorldPos = new Vector2Int( );

    public void Init( ) {
        if (camera == null)
            throw new InvalidOperationException( "Camera null" );

        if (mapGenerator == null)
            throw new InvalidOperationException( "Map generator null" );

        chunkManager = mapGenerator.GetChunkManager( );
        cursorManager = new CursorManager( camera );
    }

    private void Update( ) {
        vCursorWorldPos = cursorManager.GetPositionInChunk( );
    }

    private void OnDrawGizmos( ) {
        Gizmos.color = new Color( 1, 1, 1, 1 );
        Gizmos.DrawWireCube( new Vector3( vCursorWorldPos.x + 0.5f, vCursorWorldPos.y + 0.5f, 0 ), new Vector3( 1, 1, 1 ) );
    }
}