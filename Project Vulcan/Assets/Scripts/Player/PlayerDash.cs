using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    public static event Action OnDash;

    [Header("Dash Stats")]
    [SerializeField, Range(0f, 100f)] private float _dashSpeed = 25f;
    [SerializeField, Range(0f, 3f)] private float _dashTime = 0.5f;
    [SerializeField, Range(0f, 3f)] private float _dashCooldown = 1f;

    private Vector2 _dashDirection;

    private float _dashTimeCoutner;
    private float _lastDash;

    private bool _isDashing = false;
    private bool _canDash = true;

    public bool IsDashing => _isDashing;

    private void OnEnable()
    {
        PlayerGround.OnGroundTouch += EnableDash;
    }

    private void OnDisable()
    {
        PlayerGround.OnGroundTouch -= EnableDash;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!_canDash) return;

        _isDashing = true;
        _canDash = false;
        _dashDirection = Player.Instance.Movement.MoveInput;

        if (_dashDirection == Vector2.zero)
        {
            _dashDirection = new Vector2(transform.localScale.x, 0f);
        }

        StartCoroutine(StartDashing());
    }

    private IEnumerator StartDashing()
    {
        OnDash?.Invoke();

        _dashTimeCoutner = _dashTime;
        _lastDash = Time.time;

        while (_dashTimeCoutner > 0f)
        {
            Player.Instance.Body.velocity = _dashDirection.normalized * _dashSpeed;
            _dashTimeCoutner -= Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
    }

    private void EnableDash()
    {
        if (Time.time > _lastDash + _dashCooldown)
            _canDash = true;
    }
}