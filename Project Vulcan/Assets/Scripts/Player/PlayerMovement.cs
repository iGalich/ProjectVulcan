using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)] [Tooltip("Maximum movement speed")] private float _maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to reach max speed")] private float _maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop after letting go")] private float _maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop when changing direction")] private float _maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to reach max speed when in mid-air")] private float _maxAirAcceleration;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop in mid-air when no direction is used")] private float _maxAirDeceleration;
    [SerializeField, Range(0f, 100f)] [Tooltip("How fast to stop when changing direction when in mid-air")] private float _maxAirTurnSpeed = 80f;

    [SerializeField] [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] private bool _useAcceleration;

    private Vector2 _desiredVelocity;
    private Vector2 _velocity;
    private Vector2 _moveInput;

    private float _maxSpeedChange;
    private float _acceleration;
    private float _deceleration;
    private float _turnSpeed;

    private bool _onGround;
    private bool _pressingKey;

    public Vector2 MoveInput { get => _moveInput; set => _moveInput = value; }

    private void Update()
    {
        // Used to stop movement when the character is playing the death animation
        if (!Player.Instance.CanMove)
        {
            _moveInput.x = 0f;
        }

        // Used to flip characters sprite
        if (_moveInput.x != 0f)
        {
            transform.localScale = new Vector3(_moveInput.x > 0 ? 1 : -1, 1, 1);
            _pressingKey = true;
        }
        else
        {
            _pressingKey = false;
        }

        // Calculate characters desired velocity
        _desiredVelocity = new Vector2(_moveInput.x, 0f) * Mathf.Max(_maxSpeed - Player.Instance.Ground.Friction, 0f);
    }

    private void FixedUpdate()
    {
        float delta = Time.deltaTime;

        _onGround = Player.Instance.Ground.OnGround;
        _velocity = Player.Instance.Body.velocity;

        if (_useAcceleration)
        {
            RunWithAcceleration(delta);
        }
        else
        {
            if (_onGround)
            {
                RunWithoutAcceleration();
            }
            else
            {
                RunWithAcceleration(delta);
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (Player.Instance.CanMove)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }

    private void RunWithAcceleration(float delta)
    {
        _acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
        _deceleration = _onGround ? _maxDecceleration : _maxAirDeceleration;
        _turnSpeed = _onGround ? _maxTurnSpeed : _maxAirTurnSpeed;

        if (_pressingKey)
        {
            // If the sign of our input direction doesn't match our movement, it means we're turning aroung and so should be using the turn speed stat
            if (Mathf.Sign(_moveInput.x) != Mathf.Sign(_velocity.x))
            {
                _maxSpeedChange = _turnSpeed * delta;
            }
            else
            {
                _maxSpeedChange = _acceleration * delta;
            }
        }
        else
        {
            _maxSpeedChange = _deceleration * delta;
        }

        _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, _maxSpeedChange);
        Player.Instance.Body.velocity = _velocity;
    }

    private void RunWithoutAcceleration()
    {
        _velocity.x = _desiredVelocity.x;
        Player.Instance.Body.velocity = _velocity;
    }
}