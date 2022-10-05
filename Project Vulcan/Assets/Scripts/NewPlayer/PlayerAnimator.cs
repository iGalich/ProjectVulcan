using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region Components

    [SerializeField] private GameObject _sprite;

    private PlayerMovement _playerMovement;
    private Animator _anim;
    private SpriteRenderer _spriteRend;

    #endregion

    [Header("Movement Tilt")]
    [SerializeField] private float _maxTilt;
    [SerializeField] [Range(0, 1)] private float _tiltSpeed;

    [Header("Settings - Squash and Stretch")]
    [SerializeField] private bool _doSquashAndStretch = true;
    [Space(5)]
    [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] private Vector3 _jumpSquashSettings;
    [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] private Vector3 _landSquashSettings;
    [SerializeField, Tooltip("How powerful should the effect be?")] private float _landSqueezeMultiplier;
    [SerializeField, Tooltip("How powerful should the effect be?")] private float _jumpSqueezeMultiplier;
    [SerializeField] float _landDrop = 1;

    [Header("Particle FX")]
    [SerializeField] private bool _useFX;
    [Space(5)]
    [SerializeField] private GameObject _jumpFX;
    [SerializeField] private GameObject _landFX;
    private ParticleSystem _jumpParticle;
    private ParticleSystem _landParticle;

    private bool _startedJumping;
    private bool _justLanded;
    private bool _landSqueezing;
    private bool _jumpSqueezing;
    private bool _isSqueezing;

    private float _currentVelY;

    public bool StartedJumping { get => _startedJumping; set => _startedJumping = value; }
    public bool JustLanded { get => _justLanded; set => _justLanded = value; }

    private void Start()
    {
        GetComponents();
        SetParticleSettings();
    }

    private void LateUpdate()
    {
        Tilt();

        CheckAnimationState();
    }

    private void SetParticleSettings()
    {
        if (!_useFX) return;

        ParticleSystem.MainModule jumpPSettings = _jumpParticle.main;
        jumpPSettings.startColor = new ParticleSystem.MinMaxGradient(Color.red);

        ParticleSystem.MainModule landPSettings = _landParticle.main;
        landPSettings.startColor = new ParticleSystem.MinMaxGradient(Color.green);
    }

    private void GetComponents()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _spriteRend = GetComponentInChildren<SpriteRenderer>();
        _anim = _spriteRend.GetComponent<Animator>();

        if (!_useFX) return;

        _jumpParticle = _jumpFX.GetComponent<ParticleSystem>();
        _landParticle = _landFX.GetComponent<ParticleSystem>();
    }

    private void Tilt()
    {
        float tiltProgress;
        int mult = -1;

        if (_playerMovement.IsSliding)
        {
            tiltProgress = 0.25f;
        }
        else
        {
            tiltProgress = Mathf.InverseLerp(-_playerMovement.Data.MaxRunSpeed, _playerMovement.Data.MaxRunSpeed, _playerMovement.Body.velocity.x);
            mult = (_playerMovement.IsFacingRight) ? 1 : -1;
        }

        float newRot = (2 * tiltProgress * _maxTilt) - _maxTilt;
        float rot = Mathf.LerpAngle(_spriteRend.transform.localRotation.eulerAngles.z * mult, newRot, _tiltSpeed);

        _spriteRend.transform.localRotation = Quaternion.Euler(0, 0, rot * mult);
    }

    private void CheckAnimationState()
    {
        if (_startedJumping)
        {
            if (_useFX)
            {
                //_anim.SetTrigger("Jump");
                GameObject obj = Instantiate(_jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
                Destroy(obj, 1);
            }

            _startedJumping = false;

            if (!_jumpSqueezing && _jumpSqueezeMultiplier > 1f)
            {
                StartCoroutine(JumpSqueeze(_jumpSquashSettings.x / _jumpSqueezeMultiplier, _jumpSquashSettings.y * _jumpSqueezeMultiplier, _jumpSquashSettings.z, 0f, true));
            }

            return;
        }

        if (_justLanded)
        {
            if (_useFX)
            {
                //_anim.SetTrigger("Land");
                GameObject obj = Instantiate(_landFX, transform.position - (Vector3.up * transform.localScale.y / 1.5f), Quaternion.Euler(-90, 0, 0));
                Destroy(obj, 1);
            }

            _justLanded = false;

            if (!_landSqueezing && _landSqueezeMultiplier > 1f)
            {
                StartCoroutine(JumpSqueeze(_landSquashSettings.x * _landSqueezeMultiplier, _landSquashSettings.y / _landSqueezeMultiplier, _landSquashSettings.z, _landDrop, false));
            }

            return;
        }

        //_anim.SetFloat("Vel Y", _playerMovement.Body.velocity.y);
    }

    private IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds, float dropAmount, bool jumpSqueeze)
    {
        //We log that the player is squashing/stretching, so we don't do these calculations more than once
        if (jumpSqueeze) 
        { 
            _jumpSqueezing = true; 
        }
        else 
        { 
            _landSqueezing = true;
        }

        _isSqueezing = true;

        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);

        Vector3 originalPosition = Vector3.zero;
        Vector3 newPosition = new Vector3(0, -dropAmount, 0);

        //We very quickly lerp the character's scale and position to their squashed and stretched pose...
        float t = 0f;

        while (t <= 1f)
        {
            t += Time.deltaTime / 0.01f;

            _sprite.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            _sprite.transform.localPosition = Vector3.Lerp(originalPosition, newPosition, t);

            yield return null;
        }

        //And then we lerp back to the original scale and position at a speed dicated by the developer
        //It's important to do this to the character's sprite, not the gameobject with a Rigidbody an/or collision detection
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;

            _sprite.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            _sprite.transform.localPosition = Vector3.Lerp(newPosition, originalPosition, t);

            yield return null;
        }

        if (jumpSqueeze)
        { 
            _jumpSqueezing = false; 
        }
        else
        { 
            _landSqueezing = false; 
        }
    }
}