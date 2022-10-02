using UnityEngine;

[CreateAssetMenu(fileName = "AIController", menuName = "InputController/AIController")]
public class AIController : InputController
{
    public override bool GetJumpHoldInput()
    {
        return false;
    }

    public override bool GetJumpInput()
    {
        return true;
    }

    public override float GetMoveInput()
    {
        return 1f;
    }
}