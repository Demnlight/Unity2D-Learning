using System;
using UnityEngine;

namespace Scripts.Build {
    using AdditionalMath;

    public interface ICursorManager {
        Vector2Int GetPositionInChunk( );
    }

    public class CursorManager : ICursorManager {
        private Camera camera = null;

        public CursorManager( Camera camera ) {
            this.camera = camera;
        }

        public Vector2Int GetPositionInChunk( ) {
            Vector3 vMouseWorldPos = camera.ScreenToWorldPoint( Input.mousePosition );
            Vector2Int vFlooredPos = new Vector2Int( AdditionalMath.RoundFrom( vMouseWorldPos.x ), AdditionalMath.RoundFrom( vMouseWorldPos.y ) );
            return vFlooredPos;
        }
    }
}