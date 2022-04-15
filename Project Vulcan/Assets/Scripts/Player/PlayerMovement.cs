using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _jumpPower = 20f;
    private float _horizontalInput;

    [Header ("Layers")]
    [SerializeField] private LayerMask _groundLayer;

    // References
    private CapsuleCollider2D _collider;
    private Rigidbody2D _body;

    private void Awake() 
    {
        _collider = GetComponent<CapsuleCollider2D>();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update() 
    {
        _horizontalInput = Input.GetAxis("Horizontal");

        BasicMovement(_horizontalInput);

        // Sprite Flip

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    private void Jump()
    {
        if (IsGrounded())
            _body.velocity = new Vector2(_body.velocity.x, _jumpPower);
    }
    
    private void BasicMovement(float input)
    {
        _body.velocity = new Vector2(_horizontalInput * _speed, _body.velocity.y);
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0 , Vector2.down, 0.1f, _groundLayer);
        return raycastHit.collider != null;
    }
}