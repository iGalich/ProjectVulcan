using UnityEngine;

public class PlayerMoveLimit : MonoBehaviour
{
    [SerializeField] private bool _canMove = true;

    public bool CanMove => _canMove;
}