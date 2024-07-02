using UnityEngine;

namespace Scripts.Perlin {
    using AdditionalMath;

    public struct PerlinGeneratorSettings {
        public float flPerlinScale;
        public int nPerlinOctaves;
        public float flPersistence;
        public float flLacunarity;
        public float flPerlinBaseAmplitude;
        public Vector2[ ] vRandomOffsets;
    }
    public interface IPerlinGenerator {
        float GetHeight( Vector2Int vGlobalPos, Vector2Int vLocalPos );
    }
    public class PerlinGenerator : IPerlinGenerator {
        public PerlinGeneratorSettings Settings = new PerlinGeneratorSettings( );

        public float GetHeight( Vector2Int vGlobalPos, Vector2Int vLocalPos ) {
            float flHeight = 0.0f;
            float flAmplitude = Settings.flPerlinBaseAmplitude;
            float flFreq = 1;

            for (int i = 0; i < Settings.nPerlinOctaves; i++) {
                float px = (vGlobalPos.x + vLocalPos.x) / Settings.flPerlinScale * flFreq + Settings.vRandomOffsets[ i ].x;
                float py = (vGlobalPos.y + vLocalPos.y) / Settings.flPerlinScale * flFreq + Settings.vRandomOffsets[ i ].y;

                float PerlinValue = Mathf.PerlinNoise( px, py ) * 2 - 1;
                flHeight += PerlinValue * flAmplitude;

                flAmplitude *= Settings.flPersistence;
                flFreq *= Settings.flLacunarity;
            }

            flHeight = Mathf.InverseLerp( -1f, 1f, flHeight );
            flHeight = (float)System.Math.Pow( flHeight, 1.5f );
            flHeight = AdditionalMath.RoundUp( flHeight, 2 );

            return flHeight;
        }
    }
}