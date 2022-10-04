using UnityEngine;
using System.IO;

public class ToTextFile : MonoBehaviour
{
    [SerializeField] private PlayerMovement _player;
    private static int index = 0;

    private void Start()
    {
        Directory.CreateDirectory($"{Application.streamingAssetsPath}/SavedPlayerData");
    }

    public void CreateTextFile()
    {
        string documentName = $"{Application.streamingAssetsPath}/SavedPlayerData/Data{index++}.txt";

        File.AppendAllText(documentName, $"FallGravityMult: {_player.Data.FallGravityMult}\n");
        File.AppendAllText(documentName, $"MaxFallSpeed: {_player.Data.MaxFallSpeed}\n");
        File.AppendAllText(documentName, $"FastFallGravityMult: {_player.Data.FastFallGravityMult}\n");
        File.AppendAllText(documentName, $"MaxFastFallSpeed: {_player.Data.MaxFastFallSpeed}\n");

        File.AppendAllText(documentName, $"MaxRunSpeed: {_player.Data.MaxRunSpeed}\n");
        File.AppendAllText(documentName, $"RunAcceleration: {_player.Data.RunAcceleration}\n");
        File.AppendAllText(documentName, $"RunDecceleration: {_player.Data.RunDecceleration}\n");
        File.AppendAllText(documentName, $"AccelInAir: {_player.Data.AccelInAir}\n");
        File.AppendAllText(documentName, $"DeccelInAir: {_player.Data.DeccelInAir}\n");
        File.AppendAllText(documentName, $"DoConserveMomentum: {_player.Data.DoConserveMomentum}\n");

        File.AppendAllText(documentName, $"JumpHeight: {_player.Data.JumpHeight}\n");
        File.AppendAllText(documentName, $"JumpTimeToApex: {_player.Data.JumpTimeToApex}\n");

        File.AppendAllText(documentName, $"JumpCutGravityMult: {_player.Data.JumpCutGravityMult}\n");
        File.AppendAllText(documentName, $"JumpHangGravityMult: {_player.Data.JumpHangGravityMult}\n");
        File.AppendAllText(documentName, $"JumpHangTimeThreshold: {_player.Data.JumpHangTimeThreshold}\n");
        File.AppendAllText(documentName, $"JumpHangAccelerationMult: {_player.Data.JumpHangAccelerationMult}\n");
        File.AppendAllText(documentName, $"JumpHangMaxSpeedMult: {_player.Data.JumpHangMaxSpeedMult}\n");

        File.AppendAllText(documentName, $"WallJumpForce: {_player.Data.WallJumpForce}\n");
        File.AppendAllText(documentName, $"WallJumpRunLerp: {_player.Data.WallJumpRunLerp}\n");
        File.AppendAllText(documentName, $"WallJumpTime: {_player.Data.WallJumpTime}\n");
        File.AppendAllText(documentName, $"DoTurnOnWallJump: {_player.Data.DoTurnOnWallJump}\n");

        File.AppendAllText(documentName, $"SlideSpeed: {_player.Data.SlideSpeed}\n");
        File.AppendAllText(documentName, $"SlideAccel: {_player.Data.SlideAccel}\n");

        File.AppendAllText(documentName, $"CoyoteTime: {_player.Data.CoyoteTime}\n");
        File.AppendAllText(documentName, $"JumpInputBufferTime: {_player.Data.JumpInputBufferTime}\n");

        File.AppendAllText(documentName, $"DashAmount: {_player.Data.DashAmount}\n");
        File.AppendAllText(documentName, $"DashSpeed: {_player.Data.DashSpeed}\n");
        File.AppendAllText(documentName, $"DashSleepTime: {_player.Data.DashSleepTime}\n");
        File.AppendAllText(documentName, $"DashAttackTime: {_player.Data.DashAttackTime}\n");
        File.AppendAllText(documentName, $"DashEndTime: {_player.Data.DashEndTime}\n");
        File.AppendAllText(documentName, $"DashEndRunLerp: {_player.Data.DashEndRunLerp}\n");
        File.AppendAllText(documentName, $"DashRefillTime: {_player.Data.DashRefillTime}\n");
        File.AppendAllText(documentName, $"DashInputBufferTime: {_player.Data.DashInputBufferTime}\n");
        File.AppendAllText(documentName, $"DashEndSpeed: {_player.Data.DashEndSpeed}");
    }
}