using System.Collections.Generic;

namespace Scripts.Movement {

    public enum SpeedModifers {
        SWIMMING,
        SLOWED,
        NORMAl,
        BOOSTED
    }
    public class MovementConstants {
        public static Dictionary<SpeedModifers, float> aSpeedModifiersValues = new Dictionary<SpeedModifers, float> {
            {SpeedModifers.SWIMMING, 0.50f },
            {SpeedModifers.SLOWED, 0.75f },
            {SpeedModifers.NORMAl, 1.0f },
            {SpeedModifers.BOOSTED, 2.5f },
        };
    }
}