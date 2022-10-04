using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoManager : MonoBehaviour
{
    public static DemoManager Instance { get; private set; }

    [SerializeField] private PlayerData[] _playerTypes;
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private Button[] _buttons;

    private void Awake()
    {
        SingletonCheck();
    }

    private void Start()
    {
        _player.Data.CalculateValues();
        AddButtonListeners();
    }

    private void AddButtonListeners()
    {
        _buttons[0].onClick.AddListener(() => SetPlayerType(0));
        _buttons[1].onClick.AddListener(() => SetPlayerType(1));
        _buttons[2].onClick.AddListener(() => SetPlayerType(2));
        _buttons[3].onClick.AddListener(() => RandomizeData());
    }

    private void SetPlayerType(int index)
    {
        _player.Data = _playerTypes[index];
        _player.Data.CalculateValues();
    }

    private void RandomizeData()
    {
        _player.Data.FallGravityMult = Random.Range(0f, 5f);
        _player.Data.MaxFallSpeed = Random.Range(0f, 35f);
        _player.Data.FastFallGravityMult = Random.Range(0f, 5f);
        _player.Data.MaxFastFallSpeed = Random.Range(0f, 50f);

        _player.Data.MaxRunSpeed = Random.Range(0f, 50f);
        _player.Data.RunAcceleration = Random.Range(0f, 15f);
        _player.Data.RunDecceleration = Random.Range(0f, 15f);
        _player.Data.AccelInAir = Random.Range(0f, 1f);
        _player.Data.DeccelInAir = Random.Range(0f, 1f);
        _player.Data.DoConserveMomentum = Random.value < 0.5f;

        _player.Data.JumpHeight = Random.Range(0f, 10f);
        _player.Data.JumpTimeToApex = Random.Range(0f, 3f);

        _player.Data.JumpCutGravityMult = Random.Range(0f, 10f);
        _player.Data.JumpHangGravityMult = Random.Range(0f, 1f);
        _player.Data.JumpHangTimeThreshold = Random.Range(0f, 2f);
        _player.Data.JumpHangAccelerationMult = Random.Range(0f, 2f);
        _player.Data.JumpHangMaxSpeedMult = Random.Range(0f, 2f);

        _player.Data.WallJumpForce = new Vector2(Random.Range(0f, 30f), Random.Range(0f, 30f));
        _player.Data.WallJumpRunLerp = Random.Range(0f, 1f);
        _player.Data.WallJumpTime = Random.Range(0f, 1.5f);
        _player.Data.DoTurnOnWallJump = Random.value > 0.5f;

        _player.Data.SlideSpeed = Random.Range(-25f, 0f);
        _player.Data.SlideAccel = Random.Range(0f, 15f);

        _player.Data.CoyoteTime = Random.Range(0f, 1f);
        _player.Data.JumpInputBufferTime = Random.Range(0f, 0.5f);

        _player.Data.DashAmount = Random.Range(0, 200);
        _player.Data.DashSpeed = Random.Range(0f, 5f);
        _player.Data.DashSleepTime = Random.Range(0f, 1f);
        _player.Data.DashAttackTime = Random.Range(0f, 1f);
        _player.Data.DashEndTime = Random.Range(0f, 1f);
        _player.Data.DashEndRunLerp = Random.Range(0f, 1f);
        _player.Data.DashRefillTime = Random.Range(0f, 1f);
        _player.Data.DashInputBufferTime = Random.Range(0f, 1f);
        _player.Data.DashEndSpeed = new Vector2(Random.Range(25f, 25f), Random.Range(25f, 25f));
    }

    private void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SingletonCheck()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null && Instance != this)
            Destroy(gameObject);
    }
}