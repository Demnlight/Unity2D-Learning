using System;
using Scripts.AdditionalMath;
using Scripts.Build;
using Scripts.Chunks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour {
    [SerializeField] private MapGenerator mapGenerator = null;

    private Camera camera = null;
    private IChunkManager chunkManager = null;
    private ICursorManager cursorManager = null;
    private IObjectSetupper objectSetupper = null;

    private Vector2Int vCursorWorldPos = new Vector2Int( );

    public void Init( ) {
        if (mapGenerator == null)
            throw new InvalidOperationException( "Map generator null" );

        camera = Camera.main;
        if (camera == null)
            throw new InvalidOperationException( "Camera null" );

        chunkManager = mapGenerator.GetChunkManager( );
        cursorManager = new CursorManager( camera );
        objectSetupper = new ObjectSetupper( mapGenerator.GetTileMap );

        if (chunkManager == null)
            throw new InvalidOperationException( "Chunk Manager null" );

        if (cursorManager == null)
            throw new InvalidOperationException( "Cursor Manager null" );

        if (objectSetupper == null)
            throw new InvalidOperationException( "Object Setupper null" );
    }

    private void Update( ) {
        vCursorWorldPos = cursorManager.GetPositionInChunk( );
        Vector3Int v3CursorWorldPos = new Vector3Int( vCursorWorldPos.x, vCursorWorldPos.y, (int)mapGenerator.GetTileMap.transform.position.z );

        if (Input.GetMouseButtonDown( 0 )) {
            ObjectSettings_t settings = new ObjectSettings_t {
                bNewObject = false,
                pTile = mapGenerator.GetTileBase,
                vCenter = v3CursorWorldPos
            };
            objectSetupper.PlaceObject( settings );
        }
    }

    private void OnDrawGizmos( ) {
        Gizmos.color = new Color( 1, 1, 1, 1 );
        Gizmos.DrawWireCube( new Vector3( vCursorWorldPos.x + 0.5f, vCursorWorldPos.y + 0.5f, 0 ), new Vector3( 1, 1, 1 ) );
    }
}