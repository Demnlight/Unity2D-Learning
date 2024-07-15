using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.Build {

    public struct ObjectSettings_t {
        public Vector3Int vCenter;
        public TileBase pTile;
        public bool bNewObject;
    }

    public interface IObjectSetupper {
        void PlaceObject( ObjectSettings_t objectSettings );
    }

    public class ObjectSetupper : IObjectSetupper {
        private Tilemap map = null;

        public ObjectSetupper( Tilemap sourceMap ) {
            this.map = sourceMap;
        }

        public void PlaceObject( ObjectSettings_t objectSettings ) {
            this.map.SetTile( objectSettings.vCenter, objectSettings.pTile );

            if (objectSettings.bNewObject) {

            }
        }
    }
}