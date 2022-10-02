using UnityEngine;

public class Ground : MonoBehaviour
{
    private bool _onGround;
    private float _friction;

    public bool OnGround => _onGround;
    public float Friction => _friction;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateCollision(collision);
        GetFriction(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        EvaluateCollision(collision);
        GetFriction(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _onGround = false;
        _friction = 0f;
    }

    private void EvaluateCollision(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            _onGround = normal.y >= 0.9f;
        }
    }

    private void GetFriction(Collision2D collision)
    {
        PhysicsMaterial2D mat = collision.rigidbody.sharedMaterial;

        _friction = 0f;

        if (mat != null)
        {
            _friction = mat.friction;
        }
    }
}