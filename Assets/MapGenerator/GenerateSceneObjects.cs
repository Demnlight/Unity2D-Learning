using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.MapGenerator {

    public struct TilemapGenerator_t {
        public int nTileMapsCount;
        public string[ ] aTileMapsNames;
        public Transform[ ] aParents;
        public Color[ ] aColors;
        public Material[ ] aMaterials;
        public Vector3[ ] aPositions;
    }

    public struct Tilemap_t {
        public string TileMapName;
        public Transform Parent;
        public Color color;
        public Material material;
        public Vector3 position;
    }

    public interface ITilemapGenerator {
        void SetupTilemapData( Tilemap map, Tilemap_t settings );
        Tilemap[ ] GenerateTileMaps( TilemapGenerator_t mapsSettings );
    }

    public class SceneObjectsGenerator : ITilemapGenerator {

        public void SetupTilemapData( Tilemap map, Tilemap_t settings ) {
            map.name = settings.TileMapName;
            map.transform.parent = settings.Parent;
            map.transform.position = settings.position;
            map.color = settings.color;

            var renderer = map.AddComponent<TilemapRenderer>( );
            renderer.material = settings.material;
        }

        public Tilemap[ ] GenerateTileMaps( TilemapGenerator_t mapsSettings ) {
            Tilemap[ ] maps = new Tilemap[ mapsSettings.nTileMapsCount ];

            for (int i = 0; i < maps.Length; i++) {
                maps[ i ] = new GameObject( mapsSettings.aTileMapsNames[ i ] ).AddComponent<Tilemap>( );

                Tilemap_t localSettings = new Tilemap_t {
                    TileMapName = mapsSettings.aTileMapsNames[ i ],
                    Parent = mapsSettings.aParents[ i ],
                    color = mapsSettings.aColors[ i ],
                    position = mapsSettings.aPositions[ i ],
                    material = mapsSettings.aMaterials[ i ]
                };

                this.SetupTilemapData( maps[ i ], localSettings );
            }

            return maps;
        }
    }
}