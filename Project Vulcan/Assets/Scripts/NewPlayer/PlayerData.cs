using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
	[Header("Gravity")]
	[SerializeField, Range(0f, 5f)] private float _fallGravityMult; //Multiplier to the player's gravityScale when falling.
	[SerializeField, Range(0, 35f)] private float _maxFallSpeed; //Maximum fall speed (terminal velocity) of the player when falling.
	[SerializeField, Range(0, 5f)] private float _fastFallGravityMult; //Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed. Seen in games such as Celeste, lets the player fall extra fast if they wish.
	[SerializeField, Range(0, 50f)] private float _maxFastFallSpeed; //Maximum fall speed(terminal velocity) of the player when performing a faster fall.

	private float _gravityStrength; //Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
	private float _gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D). Also the value the player's rigidbody2D.gravityScale is set to.

	[Space(20)]

	[Header("Run")]
	[SerializeField, Range(0f, 50f)] private float _maxRunSpeed; //Target speed we want the player to reach.
	[SerializeField, Range(0f, 15f)] private float _runAcceleration; //The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all
	[SerializeField, Range(0, 15f)] private float _runDecceleration; //The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all
	[SerializeField, Range(0f, 1f)] private float _accelInAir; //Multipliers applied to acceleration rate when airborne.
	[SerializeField, Range(0f, 1f)] private float _deccelInAir;

	[SerializeField] private bool _doConserveMomentum = true;

	private float _runAccelAmount; //The actual force (multiplied with speedDiff) applied to the player.
	private float _runDeccelAmount; //Actual force (multiplied with speedDiff) applied to the player.

	[Space(20)]

	[Header("Jump")]
	[SerializeField, Range(0f, 10f)] private float _jumpHeight; //Height of the player's jump
	[SerializeField, Range(0f, 3f)] private float _jumpTimeToApex; //Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.

	private float _jumpForce; //The actual force applied (upwards) to the player when they jump.

	[Header("Both Jumps")]
	[SerializeField, Range(0f, 10f)] private float _jumpCutGravityMult; //Multiplier to increase gravity if the player releases thje jump button while still jumping
	[SerializeField, Range(0f, 1f)] private float _jumpHangGravityMult; //Reduces gravity while close to the apex (desired max height) of the jump
	[SerializeField, Range(0f, 2f)] private float _jumpHangTimeThreshold; //Speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
	[SerializeField, Range(0f, 2f)] private float _jumpHangAccelerationMult;
	[SerializeField, Range(0f, 2f)] private float _jumpHangMaxSpeedMult;

	[Header("Wall Jump")]
	[SerializeField] private Vector2 _wallJumpForce; //The actual force (this time set by us) applied to the player when wall jumping.

	[SerializeField, Range(0f, 1f)] private float _wallJumpRunLerp; //Reduces the effect of player's movement while wall jumping.
	[SerializeField, Range(0f, 1.5f)] private float _wallJumpTime; //Time after wall jumping the player's movement is slowed for.

	[SerializeField] private bool _doTurnOnWallJump; //Player will rotate to face wall jumping direction

	[Space(20)]

	[Header("Slide")]
	[SerializeField, Range(-25f, 0f)] private float _slideSpeed;
	[SerializeField, Range(0f, 15f)] private float _slideAccel;

	[Header("Assists")]
	[SerializeField, Range(0f, 1f)] private float _coyoteTime; //Grace period after falling off a platform, where you can still jump
	[SerializeField, Range(0f, 0.5f)] private float _jumpInputBufferTime; //Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.

	[Space(20)]

	[Header("Dash")]
	[SerializeField, Range(0, 200)] private int _dashAmount;

	[SerializeField, Range(0f, 5f)] private float _dashSpeed;
	[SerializeField, Range(0f, 1f)] private float _dashSleepTime; //Duration for which the game freezes when we press dash but before we read directional input and apply a force
	[SerializeField, Range(0f, 1f)] private float _dashAttackTime;
	[SerializeField, Range(0f, 1f)] private float _dashEndTime; //Time after you finish the inital drag phase, smoothing the transition back to idle (or any standard state)
	[SerializeField, Range(0f, 1f)] private float _dashEndRunLerp; //Slows the affect of player movement while dashing
	[SerializeField, Range(0f, 1f)] private float _dashRefillTime;
	[SerializeField, Range(0.01f, 0.5f)] private float _dashInputBufferTime;
	
	[SerializeField] private Vector2 _dashEndSpeed; //Slows down player, makes dash feel more responsive (used in Celeste)

	#region Getters & Setters

	public float GravityScale => _gravityScale;
	public float JumpForce => _jumpForce;
	public float JumpInputBufferTime { get => _jumpInputBufferTime; set => _jumpInputBufferTime = value; }
	public float DashInputBufferTime { get => _dashInputBufferTime; set => _dashInputBufferTime = value; }
	public float MaxRunSpeed { get => _maxRunSpeed; set => _maxRunSpeed = value; }
	public float CoyoteTime { get => _coyoteTime; set => _coyoteTime = value; }
	public float WallJumpTime { get => _wallJumpTime; set => _wallJumpTime = value; }
	public float DashAttackTime { get => _dashAttackTime; set => _dashAttackTime = value; }
	public float DashSpeed { get => _dashSpeed; set => _dashSpeed = value; }
	public float DashSleepTime { get => _dashSleepTime; set => _dashSleepTime = value; }
	public float SlideSpeed { get => _slideSpeed; set => _slideSpeed = value; }
	public float FastFallGravityMult { get => _fastFallGravityMult; set => _fastFallGravityMult = value; }
	public float MaxFastFallSpeed { get => _maxFastFallSpeed; set => _maxFastFallSpeed = value; }
	public float JumpCutGravityMult { get => _jumpCutGravityMult; set => _jumpCutGravityMult = value; }
	public float MaxFallSpeed { get => _maxFallSpeed; set => _maxFallSpeed = value; }
	public float JumpHangTimeThreshold { get => _jumpHangTimeThreshold; set => _jumpHangTimeThreshold = value; }
	public float JumpHangGravityMult { get => _jumpHangGravityMult; set => _jumpHangGravityMult = value; }
	public float FallGravityMult { get => _fallGravityMult; set => _fallGravityMult = value; }
	public float WallJumpRunLerp { get => _wallJumpRunLerp; set => _wallJumpRunLerp = value; }
	public float DashEndRunLerp { get => _dashEndRunLerp; set => _dashEndRunLerp = value; }
	public float RunAccelAmount => _runAccelAmount;
	public float RunDeccelAmount => _runDeccelAmount;
	public float AccelInAir { get => _accelInAir; set => _accelInAir = value; }
	public float DeccelInAir { get => _deccelInAir; set => _deccelInAir = value; }
	public bool DoConserveMomentum { get => _doConserveMomentum; set => _doConserveMomentum = value; }
	public float JumpHangMaxSpeedMult { get => _jumpHangMaxSpeedMult; set => _jumpHangMaxSpeedMult = value; }
	public float JumpHangAccelerationMult { get => _jumpHangAccelerationMult; set => _jumpHangAccelerationMult = value; }
	public Vector2 WallJumpForce { get => _wallJumpForce; set => _wallJumpForce = value; }
	public Vector2 DashEndSpeed { get => _dashEndSpeed; set => _dashEndSpeed = value; }
	public float DashEndTime { get => _dashEndTime; set => _dashEndTime = value; }
	public float DashRefillTime { get => _dashRefillTime; set => _dashRefillTime = value; }
	public int DashAmount { get => _dashAmount; set => _dashAmount = value; }
	public float SlideAccel { get => _slideAccel; set => _slideAccel = value; }
	public float RunAcceleration { get => _runAcceleration; set => _runAcceleration = value; }
	public float RunDecceleration { get => _runDecceleration; set => _runDecceleration = value; }
	public float JumpHeight { get => _jumpHeight; set => _jumpHeight = value; }
	public float JumpTimeToApex { get => _jumpTimeToApex; set => _jumpTimeToApex = value; }
	public bool DoTurnOnWallJump { get => _doTurnOnWallJump; set => _doTurnOnWallJump = value; }


    #endregion

    //Unity Callback, called when the inspector updates
    private void OnValidate()
	{
		CalculateValues();	
	}

	public void CalculateValues()
    {
		//Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
		_gravityStrength = -(2 * _jumpHeight) / Mathf.Pow(_jumpTimeToApex, 2);

		//Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
		_gravityScale = _gravityStrength / Physics2D.gravity.y;

		//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		_runAccelAmount = (50 * _runAcceleration) / _maxRunSpeed;
		_runDeccelAmount = (50 * _runDecceleration) / _maxRunSpeed;

		//Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
		_jumpForce = Mathf.Abs(_gravityStrength) * _jumpTimeToApex;

		#region Variable Ranges
		_runAcceleration = Mathf.Clamp(_runAcceleration, 0.01f, _maxRunSpeed);
		_runDecceleration = Mathf.Clamp(_runDecceleration, 0.01f, _maxRunSpeed);
		#endregion
	}
}