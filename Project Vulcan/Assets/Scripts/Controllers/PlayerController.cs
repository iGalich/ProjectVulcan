using UnityEngine;

[CreateAssetMenu(fileName = "PlayerController", menuName = "InputController/PlayerController")]
public class PlayerController : InputController
{
    public override bool GetJumpHoldInput()
    {
        return Input.GetButton("Jump");
    }

    public override bool GetJumpInput()
    {
        return Input.GetButtonDown("Jump");
    }

    public override float GetMoveInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }
}