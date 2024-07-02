using System;

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
    }
}