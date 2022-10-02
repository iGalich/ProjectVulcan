using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    #region Components

    private Rigidbody2D _body;
    private PlayerGround _ground;
    private PlayerJuice _juice;
    private PlayerMoveLimit _moveLimit;

    #endregion

    [Header("Jump Stats")]
    [SerializeField, Range(2f, 5.5f)] [Tooltip("Max jump height")] private float _jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)] [Tooltip("How long it takes to reach max height before coming down")] private float _timeToJumpApex;
    [SerializeField, Range(0f, 5f)] [Tooltip("Gravity multiplier to apply when going up")] private float _upwardMoveMulti = 1f;
    [SerializeField, Range(1f, 10f)] [Tooltip("Gravity multiplier to apply when going down")] private float _downwardMoveMulti = 6f;
    [SerializeField, Range(0, 1)] [Tooltip("How many times you can jump in the air")] private int _maxAirJumps = 0;
    [SerializeField, Range(1f, 10f)] [Tooltip("Gravity multiplier when you let go of jump button")] private float _jumpCutOff;
    [SerializeField] [Tooltip("Speed limit of how fast you can fall")] private float _speedLimit;
    [SerializeField, Range(0f, 0.3f)] [Tooltip("How long should coyote time last")] private float _coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)] [Tooltip("How far from the ground should jump be cached")] private float _jumpBuffer = 0.15f;

    private Vector2 _velocity;
    private float _jumpSpeed;
    private float _defaultGravityScale;
    private float _gravityMultiplier;
    private float _jumpBufferCounter;
    private float _coyoteTimeCounter = 0f;

    private bool _canJumpAgain = false;
    private bool _desiresJump;
    private bool _pressingJump;
    private bool _onGround;
    private bool _isJumping;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _ground = GetComponent<PlayerGround>();
        _juice = GetComponent<PlayerJuice>();
        _moveLimit = GetComponent<PlayerMoveLimit>();

        _defaultGravityScale = 1f;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        SetPhysics();

        _onGround = _ground.OnGround;

        if (_jumpBuffer > 0f)
        {
            if (_desiresJump)
            {
                _jumpBufferCounter += delta;

                if (_jumpBufferCounter > _jumpBuffer)
                {
                    _desiresJump = false;
                    _jumpBufferCounter = 0f;
                }
            }
        }

        if (!_isJumping && !_onGround)
        {
            _coyoteTimeCounter += delta;
        }
        else
        {
            _coyoteTimeCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        _velocity = _body.velocity;

        if (_desiresJump)
        {
            PerformJump();
            _body.velocity = _velocity;
            return;
        }

        CalculateGravity();
    }

    private void CalculateGravity()
    {
        if (_body.velocity.y > 0.01f)
        {
            if (_onGround)
            {
                _gravityMultiplier = _defaultGravityScale;
            }
            else
            {
                if (_pressingJump && _isJumping)
                {
                    _gravityMultiplier = _upwardMoveMulti;
                }
                else
                {
                    _gravityMultiplier = _jumpCutOff;
                }
            }
        }
        else if (_body.velocity.y < -0.01f)
        {
            if (_onGround)
            {
                _gravityMultiplier = _defaultGravityScale;
            }
            else
            {
                _gravityMultiplier = _downwardMoveMulti;
            }
        }
        else
        {
            if (_onGround)
            {
                _isJumping = false;
            }

            _gravityMultiplier = _defaultGravityScale;
        }

        _body.velocity = new Vector3(_velocity.x, Mathf.Clamp(_velocity.y, -_speedLimit, 100f));
    }

    private void PerformJump()
    {
        if (_onGround || (_coyoteTimeCounter > 0.03f && _coyoteTimeCounter < _coyoteTime) || _canJumpAgain)
        {
            _desiresJump = false;
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;

            _canJumpAgain = (_maxAirJumps == 1 && !_canJumpAgain);
            _jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _body.gravityScale * _jumpHeight);

            if (_velocity.y > 0f)
            {
                _jumpSpeed = Mathf.Max(_jumpSpeed - _velocity.y, 0f);
            }
            else if (_velocity.y < 0f)
            {
                _jumpSpeed += Mathf.Abs(_body.velocity.y);
            }

            _velocity.y += _jumpSpeed;
            _isJumping = true;

            if (_juice != null)
            {
                _juice.JumpEffects();
            }
        }

        if (_jumpBuffer == 0)
        {
            _desiresJump = false;
        }
    }

    public void BounceUp(float amount)
    {
        _body.AddForce(Vector2.up * amount, ForceMode2D.Impulse);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (_moveLimit.CanMove)
        {
            if (context.started)
            {
                _desiresJump = true;
                _pressingJump = true;
            }

            if (context.canceled)
            {
                _pressingJump = false;
            }
        }
    }

    private void SetPhysics()
    {
        Vector2 newGravity = new Vector2(0f, (-2f * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2));
        _body.gravityScale = (newGravity.y / Physics2D.gravity.y) * _gravityMultiplier;
    }
}