using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private PlayerController _playerInput;

    #region Components

    private Rigidbody2D _body;
    private PlayerMovement _movement;
    private PlayerGround _ground;
    private PlayerJuice _juice;
    private PlayerJump _playerJump;

    #endregion

    [SerializeField] private bool _canMove = true;

    public PlayerController PlayerInput => _playerInput;
    public Rigidbody2D Body { get => _body; set => _body = value; }
    public PlayerGround Ground => _ground;
    public PlayerJuice Juice => _juice;
    public bool CanMove => _canMove;

    private void Awake()
    {
        SingletonCheck();
        GetComponents();
    }

    private void OnEnable()
    {
        SetUpPlayerInput();

        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Disable();
    }

    private void SetUpPlayerInput()
    {
        if (_playerInput == null)
        {
            _playerInput = new PlayerController();

            _playerInput.Player.Move.performed += _playerInput => _movement.MoveInput = _playerInput.ReadValue<Vector2>();
            _playerInput.Player.Jump.performed += _playerJump.Jump;
        }
    }

    private void SingletonCheck()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null && Instance != this)
            Destroy(gameObject);
    }
    
    private void GetComponents()
    {
        _body = GetComponent<Rigidbody2D>();
        _movement = GetComponent<PlayerMovement>();
        _ground = GetComponent<PlayerGround>();
        _juice = GetComponent<PlayerJuice>();
        _playerJump = GetComponent<PlayerJump>();
    }
}