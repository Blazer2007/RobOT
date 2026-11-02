// MovementData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "RobOT/Movement Data", fileName = "MovementData")]
public class MovementData : ScriptableObject
{
    // Velocidade
    [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;
    [field: SerializeField] public float Acceleration { get; private set; } = 40f;
    [field: SerializeField] public float Deceleration { get; private set; } = 50f;
    [field: SerializeField] public float AirControl { get; private set; } = 0.5f;

    // Salto e gravidade
    [field: SerializeField] public float JumpHeight { get; private set; } = 1.5f;
    [field: SerializeField] public float FallGravityMultiplier { get; private set; } = 2f;

    // Rotação e chão
    [field: SerializeField] public float TurnSpeed { get; private set; } = 720f;
    [field: SerializeField] public float MaxSlopeAngle { get; private set; } = 45f;
    [field: SerializeField] public float GroundCheckDistance { get; private set; } = 0.1f;
}