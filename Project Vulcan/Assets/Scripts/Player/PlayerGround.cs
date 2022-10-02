using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    private bool _onGround;

    [Header("Collider Settings")]
    [SerializeField] [Tooltip("Length of the ground-checking collider")] private float _groundLength = 0.95f;
    [SerializeField] [Tooltip("Distance between the ground-checking colliders")] private Vector3 _colliderOffset;

    [Header("Layer Masks")]
    [SerializeField] [Tooltip("Which layers are read as the ground")] private LayerMask _groundLayer;

    public bool OnGround => _onGround;

    private void Update()
    {
        _onGround = Physics2D.Raycast(transform.position + _colliderOffset, Vector2.down, _groundLength, _groundLayer) || Physics2D.Raycast(transform.position - _colliderOffset, Vector2.down, _groundLength, _groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (_onGround)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(transform.position + _colliderOffset, transform.position + _colliderOffset + Vector3.down * _groundLength);
        Gizmos.DrawLine(transform.position - _colliderOffset, transform.position - _colliderOffset + Vector3.down * _groundLength);
    }
}