using Fusion;
using UnityEngine;

// Dùng chung cái này cho cả LocalInputHandler và PlayerController
public struct PlayerInputData : INetworkInput
{
    public Vector2 MovementDirection;
    public NetworkBool IsAttackPressed; // Thêm dòng này để dùng được nút đánh
    public bool IsRPressed;
}