using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _jumpPower = 20f;
    private float _horizontalInput;
    private bool _isInControl;

    [Header ("Coyote Time")]
    [SerializeField] private float _coyoteTime = 0.25f;
    private float _coyoteCounter;

    [Header ("Wall Jump")]
    [SerializeField] private float _wallDistance = 0.55f;
    [SerializeField] private float _wallJumpTime = 0.2f;
    private float _wallJumpCounter = Mathf.NegativeInfinity;
    private float _initialGravity;
    private bool _canGrabWall;
    private bool _isGrabbingWall;
    private RaycastHit2D _wallHit;
    private BoxCollider2D _lastWall;

    [Header ("Layers")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;

    // References
    private BoxCollider2D _collider;
    private Rigidbody2D _body;

    private void Awake() 
    {
        _collider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();
        _initialGravity = _body.gravityScale;
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxis("Horizontal");

        _isInControl = CheckIfInControl();
        if (!_isInControl)
        {
            _wallJumpCounter -= Time.deltaTime;
        }

        FlipSprite();

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        CheckForWall();
    }

    private void FixedUpdate() 
    {
        BasicMovement(_horizontalInput);

        if (IsGrounded())
            _lastWall = null;
    }

    private bool CheckIfInControl()
    {
        return _wallJumpCounter <= 0;
    }

    private void CheckForWall()
    {
        _canGrabWall = IsOnWall();
        _isGrabbingWall = false;

        if (_canGrabWall && !IsGrounded() && _lastWall != _wallHit.collider.GetComponent<BoxCollider2D>())
        {
            // TODO check if this entire line is really necessary
            _isGrabbingWall = (transform.localScale.x > 0 && _horizontalInput > 0) || (transform.localScale.x < 0 && _horizontalInput < 0);
        }

        if (_isGrabbingWall && _isInControl)
        {
            _body.gravityScale = 0;
            _body.velocity = Vector2.zero;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _lastWall = _wallHit.collider.GetComponent<BoxCollider2D>();
                _wallJumpCounter = _wallJumpTime;
                _isInControl = false;
                _body.gravityScale = _initialGravity;
                _body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * _speed, _jumpPower);
                _isGrabbingWall = false;
            }
        }
        else
        {
            _body.gravityScale = _initialGravity;
        }
    }

    private void Jump()
    {
        if (_isInControl && IsGrounded())
        {
            _body.velocity = new Vector2(_body.velocity.x, _jumpPower);
        }
    }
    
    private void BasicMovement(float input)
    {
        if (_isInControl)
            _body.velocity = new Vector2(_horizontalInput * _speed, _body.velocity.y);
    }

    private void FlipSprite()
    {
        if (_body.velocity.x > 0.01f)
            transform.localScale = Vector3.one;
        else if (_body.velocity.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private bool IsOnWall()
    {
        _wallHit = Physics2D.Raycast(transform.position, new Vector2(_wallDistance * Mathf.Sign(transform.localScale.x), 0), _wallDistance, _wallLayer);
        Debug.DrawRay(transform.position, new Vector2(_wallDistance, 0) * Mathf.Sign(transform.localScale.x), Color.red);
        return _wallHit;
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0 , Vector2.down, 0.1f, _groundLayer);
        
        return raycastHit.collider != null;
    }
}