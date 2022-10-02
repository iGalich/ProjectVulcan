using UnityEngine;

public abstract class InputController : ScriptableObject
{
    public abstract float GetMoveInput();
    public abstract bool GetJumpInput();
    public abstract bool GetJumpHoldInput();
}