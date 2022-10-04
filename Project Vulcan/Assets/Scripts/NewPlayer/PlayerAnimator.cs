using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private bool _justLanded;
    [SerializeField] private bool _startedJumping;

    public bool JustLanded { get => _justLanded; set => _justLanded = value; }
    public bool StartedJumping { get => _startedJumping; set => _startedJumping = value; }
}