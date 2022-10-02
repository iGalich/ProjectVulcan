using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    public static event Action onDash;

    [Header("Dash Stats")]
    [SerializeField, Range(0f, 50f)] private float _dashSpeed = 25f;
    [SerializeField, Range(0f, 3f)] private float _dashTime = 1f;
    [SerializeField, Range(0f, 3f)] private float _dashCooldown = 1f;

    private float _dashTimeLeft;
    private float _lastDash = float.MinValue;

    public void Dash(InputAction.CallbackContext context)
    {
        if (Player.Instance.CanMove && Time.time >= _lastDash + _dashCooldown)
        {
            Vector2 direction = Player.Instance.Movement.MoveInput.normalized;

            onDash?.Invoke();
            DashAction(direction);
        }
    }

    private void DashAction(Vector2 direction)
    {
        _lastDash = Time.time;
        _dashTimeLeft = _dashTime;

        Player.Instance.Body.velocity = direction * _dashSpeed;
    }
}