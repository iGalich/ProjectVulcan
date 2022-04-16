using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _jumpPower = 20f;
    private float _horizontalInput;

    [Header ("Coyote Time")]
    [SerializeField] private float _coyoteTime = 0.25f;
    private float _coyoteCounter;

    [Header ("Wall Jump")]
    [SerializeField] private float _wallSlideSpeed = 0.3f;
    [SerializeField] private float _wallDistance = 0.55f;
    [SerializeField] private float _wallSlideTime = 1f;
    private float _lastWallSlide = Mathf.Infinity;
    private RaycastHit2D _wallHit;
    private bool _isWallSliding = false;

    [Header ("Layers")]
    [SerializeField] private LayerMask _groundLayer;

    // References
    private BoxCollider2D _collider;
    private Rigidbody2D _body;

    private void Awake() 
    {
        _collider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        
        FlipSprite(_horizontalInput);
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    private void FixedUpdate() 
    {
        BasicMovement(_horizontalInput);
        WallCheck();
    }

    private void WallCheck()
    {
        _wallHit = Physics2D.Raycast(transform.position, new Vector2(_wallDistance * Mathf.Sign(transform.localScale.x), 0), _wallDistance, _groundLayer);
        Debug.DrawRay(transform.position, new Vector2(_wallDistance, 0), Color.red); // TODO remove

        if (!_wallHit)
        {
            _isWallSliding = false;
        }

        if (_wallHit && !IsGrounded() && !_isWallSliding)
        {
            _isWallSliding = true;
            _lastWallSlide = Time.time;
        }

        if (Time.time - _lastWallSlide < _wallSlideTime && _isWallSliding)
        {
            _body.velocity = new Vector2(_body.velocity.x, Mathf.Clamp(_body.velocity.y, _wallSlideSpeed, float.MaxValue));
        }

        if (IsGrounded())
        {
            _isWallSliding = false; 
            _coyoteCounter = _coyoteTime;
        }
        else
        {
            _coyoteCounter -= Time.deltaTime;
        }
    }

    private bool OnWall()
    {
        return _wallHit;
    }

    private void Jump()
    {
        if (IsGrounded())
            _body.velocity = new Vector2(_body.velocity.x, _jumpPower);
        //else if (OnWall())
        //    WallJump();
        else if (_coyoteCounter > 0 || OnWall())
            _body.velocity = new Vector2(_body.velocity.x, _jumpPower);
    }

    private void WallJump()
    {
        _body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x), _jumpPower);
    }
    
    private void BasicMovement(float input)
    {
        _body.velocity = new Vector2(_horizontalInput * _speed, _body.velocity.y);
    }

    private void FlipSprite(float input)
    {
        if (input > 0.01f)
            transform.localScale = Vector3.one;
        else if (input < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0 , Vector2.down, 0.1f, _groundLayer);
        return raycastHit.collider != null;
    }
}