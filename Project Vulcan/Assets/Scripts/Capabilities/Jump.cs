using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private InputController _input = null;
    [SerializeField, Range(0f, 10f)] private float _jumpHeight = 3f;
    [SerializeField, Range(0, 5)] private int _maxAirJumps = 0;
    [SerializeField, Range(0f, 5f)] private float _downwardMovementMultiplier = 3f;
    [SerializeField, Range(0f, 5f)] private float _upwardMovementMultiplier = 1.7f;
    [SerializeField, Range(0f, 0.3f)] private float _coyoteTime = 0.2f;
    [SerializeField, Range(0f, 0.3f)] private float _jumpBufferTime = 0.2f;

    private Rigidbody2D _body;
    private Ground _ground;
    private Vector2 _velocity;

    private int _jumpPhase;
    private float _defaultGravityScale;
    private float _coyoteCounter;
    private float _jumpBufferCounter;

    private bool _desiredJump;
    private bool _onGround;
    private bool _isJumping;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _ground = GetComponent<Ground>();

        _defaultGravityScale = 1f;
    }

    private void Update()
    {
        _desiredJump |= _input.GetJumpInput();
    }

    private void FixedUpdate()
    {
        _onGround = _ground.OnGround;
        _velocity = _body.velocity;

        if (_onGround && _body.velocity.y == 0)
        {
            _jumpPhase = 0;
            _coyoteCounter = _coyoteTime;
            _isJumping = false;
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
        }

        if (_desiredJump)
        {
            _desiredJump = false;
            _jumpBufferCounter = _jumpBufferTime;
        }
        else if (!_desiredJump && _jumpBufferCounter > 0f)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        if (_jumpBufferCounter > 0f)
        {
            JumpAction();
        }

        if (_input.GetJumpHoldInput() && _body.velocity.y > 0f)
        {
            _body.gravityScale = _upwardMovementMultiplier;
        }
        else if (!_input.GetJumpHoldInput() || _body.velocity.y < 0f)
        {
            _body.gravityScale = _downwardMovementMultiplier;
        }
        else if (_body.velocity.y == 0)
        {
            _body.gravityScale = _defaultGravityScale;
        }

        _body.velocity = _velocity;
    }

    private void JumpAction()
    {
        if (_coyoteCounter > 0f || (_jumpHeight < _maxAirJumps && _isJumping))
        {
            if (_isJumping)
            {
                _jumpPhase++;
            }

            _jumpBufferCounter = 0f;
            _coyoteCounter = 0f;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _jumpHeight);
            _isJumping = true;

            if (_velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - _velocity.y, 0f);
            }
            _velocity.y += jumpSpeed;
        }
    }
}