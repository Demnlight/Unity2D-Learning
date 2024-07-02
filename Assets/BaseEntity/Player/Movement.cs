using System.Collections.Generic;
using UnityEngine;

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

    public struct MovementDTO {
        public Vector2 vDirection;
        public float flMaxSpeed;
        public float flFriction;
        public Rigidbody2D rb;
    }

    public interface IMovement {
        void Move( MovementDTO movementDTO );
    }

    public class SwimmingMovement : IMovement {
        public void Move( MovementDTO movementDTO ) {
            float flLocalMaxSpeed = movementDTO.flMaxSpeed * MovementConstants.aSpeedModifiersValues[ SpeedModifers.SWIMMING ];
            Vector2 targetVelocity = movementDTO.vDirection * flLocalMaxSpeed;
            Vector2 speedDiff = targetVelocity - movementDTO.rb.velocity;
            Vector2 accelerationVector = speedDiff * Time.deltaTime;

            if (movementDTO.vDirection.magnitude == 0) {
                movementDTO.rb.velocity = new Vector2( movementDTO.rb.velocity.x * movementDTO.flFriction, movementDTO.rb.velocity.y * movementDTO.flFriction );
            } else {
                movementDTO.rb.velocity = new Vector2(
                    Mathf.Clamp( movementDTO.rb.velocity.x + accelerationVector.x, -flLocalMaxSpeed, flLocalMaxSpeed ),
                    Mathf.Clamp( movementDTO.rb.velocity.y + accelerationVector.y, -flLocalMaxSpeed, flLocalMaxSpeed )
                );
            }
        }
    }

    public class RunningMovement : IMovement {
        public void Move( MovementDTO movementDTO ) {
            float flLocalMaxSpeed = movementDTO.flMaxSpeed * MovementConstants.aSpeedModifiersValues[ SpeedModifers.BOOSTED ];
            Vector2 targetVelocity = movementDTO.vDirection * flLocalMaxSpeed;
            Vector2 speedDiff = targetVelocity - movementDTO.rb.velocity;
            Vector2 accelerationVector = speedDiff * Time.deltaTime;

            if (movementDTO.vDirection.magnitude == 0) {
                movementDTO.rb.velocity = new Vector2( movementDTO.rb.velocity.x * movementDTO.flFriction, movementDTO.rb.velocity.y * movementDTO.flFriction );
            } else {
                movementDTO.rb.velocity = new Vector2(
                    Mathf.Clamp( movementDTO.rb.velocity.x + accelerationVector.x, -flLocalMaxSpeed, flLocalMaxSpeed ),
                    Mathf.Clamp( movementDTO.rb.velocity.y + accelerationVector.y, -flLocalMaxSpeed, flLocalMaxSpeed )
                );
            }
        }
    }

    public class SlowedMovement : IMovement {
        public void Move( MovementDTO movementDTO ) {
            float flLocalMaxSpeed = movementDTO.flMaxSpeed * MovementConstants.aSpeedModifiersValues[ SpeedModifers.SLOWED ];
            Vector2 targetVelocity = movementDTO.vDirection * flLocalMaxSpeed;
            Vector2 speedDiff = targetVelocity - movementDTO.rb.velocity;
            Vector2 accelerationVector = speedDiff * Time.deltaTime;

            if (movementDTO.vDirection.magnitude == 0) {
                movementDTO.rb.velocity = new Vector2( movementDTO.rb.velocity.x * movementDTO.flFriction, movementDTO.rb.velocity.y * movementDTO.flFriction );
            } else {
                movementDTO.rb.velocity = new Vector2(
                    Mathf.Clamp( movementDTO.rb.velocity.x + accelerationVector.x, -flLocalMaxSpeed, flLocalMaxSpeed ),
                    Mathf.Clamp( movementDTO.rb.velocity.y + accelerationVector.y, -flLocalMaxSpeed, flLocalMaxSpeed )
                );
            }
        }
    }

    public class WalkingMovement : IMovement {
        public void Move( MovementDTO movementDTO ) {
            Vector2 targetVelocity = movementDTO.vDirection * movementDTO.flMaxSpeed;
            Vector2 speedDiff = targetVelocity - movementDTO.rb.velocity;
            Vector2 accelerationVector = speedDiff * Time.deltaTime;

            if (movementDTO.vDirection.magnitude == 0) {
                movementDTO.rb.velocity = new Vector2( movementDTO.rb.velocity.x * movementDTO.flFriction, movementDTO.rb.velocity.y * movementDTO.flFriction );
            } else {
                movementDTO.rb.velocity = new Vector2(
                    Mathf.Clamp( movementDTO.rb.velocity.x + accelerationVector.x, -movementDTO.flMaxSpeed, movementDTO.flMaxSpeed ),
                    Mathf.Clamp( movementDTO.rb.velocity.y + accelerationVector.y, -movementDTO.flMaxSpeed, movementDTO.flMaxSpeed )
                );
            }
        }
    }
}