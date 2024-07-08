using System;
using UnityEngine;

namespace Scripts.AdditionalMath {
    public class AdditionalMath {
        public static float RoundUp( float number, int digits ) {
            var factor = Convert.ToDouble( Math.Pow( 10, digits ) );
            var dnumber = Convert.ToDouble( number );
            return (float)(Math.Ceiling( dnumber * factor ) / factor);
        }

        public static int RoundFrom( float flFrom ) {
            int nReturn = 0;
            if (flFrom > 0.0f)
                nReturn = (int)Math.Floor( flFrom );
            else
                nReturn = -(int)Math.Ceiling( Math.Abs( flFrom ) );

            return nReturn;
        }

        public static Vector2Int GetCurrentChunkWorldPos( Vector2 vGlobalPos ) {
            Vector2Int vLocalPos = new Vector2Int {
                x = RoundFrom( vGlobalPos.x / ChunkConstants.nChunkSize ) * ChunkConstants.nChunkSize,
                y = RoundFrom( vGlobalPos.y / ChunkConstants.nChunkSize ) * ChunkConstants.nChunkSize
            };
            return vLocalPos;
        }

        public static Vector2Int GetCurrentChunkScaledPos( Vector2 vGlobalPos ) {
            Vector2Int vLocalPos = new Vector2Int {
                x = RoundFrom( vGlobalPos.x / ChunkConstants.nChunkSize ),
                y = RoundFrom( vGlobalPos.y / ChunkConstants.nChunkSize )
            };
            return vLocalPos;
        }
    }
}