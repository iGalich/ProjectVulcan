using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
	//Scriptable object which holds all the player's movement parameters. If you don't want to use it
	//just paste in all the parameters, though you will need to manuly change all references in this script
	[SerializeField] private PlayerData _data;

	private PlayerController _playerController;

	#region COMPONENTS

	private Rigidbody2D _body;
	private PlayerAnimator _playerAnimator;

	#endregion

	#region STATE PARAMETERS

	// Variables control the various actions the player can perform at any time.

	private bool _isFacingRight;
	private bool _isJumping;
	private bool _isWallJumping;
	private bool _isDashing;
	private bool _isSliding;
	private bool _isOnGround;

	// Timers (also all fields, could be private and a method returning a bool could be used)
	private float _lastTimeOnGround;
	private float _lastTimeOnWall;
	private float _lastTimeOnRightWall;
	private float _lastTimeOnLeftWall;

	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;

	//Wall Jump
	private float _wallJumpStartTime;

	private int _lastWallJumpDir;

	//Dash
	private int _dashesLeft;

	private Vector2 _lastDashDir;

	private bool _dashRefilling;
	private bool _isDashAttacking;

	#endregion

	#region INPUT PARAMETERS

	private Vector2 _moveInput;

	private float _lastTimePressedJump;
	private float _lastTimePressedDash;

	#endregion

	#region CHECK PARAMETERS

	//Set all of these up in the inspector
	[Header("Ground Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);

	[Header("Wall Checks")]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

	#endregion

	#region LAYERS & TAGS

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;

    #endregion

    #region Getters & Setters

    public PlayerData Data { get => _data; set => _data = value; }
	public Rigidbody2D Body => _body;
	public bool IsSliding => _isSliding;
	public bool IsFacingRight => _isFacingRight;
	public bool IsOnGround => _isOnGround;

    #endregion

    private void Awake()
	{
		_body = GetComponent<Rigidbody2D>();
		_playerAnimator = GetComponent<PlayerAnimator>();
	}

	private void Start()
	{
		SetGravityScale(_data.GravityScale);
		_isFacingRight = true;
	}

    private void OnEnable()
    {
		SetUpPlayerInput();

		_playerController.Player.Enable();
    }

    private void Update()
	{
		float delta = Time.deltaTime;

		HandleTimers(delta);

		HandleInput();

		CheckCollisions();

		JumpChecks();

		DashChecks();

		SlideChecks();

		ManageGravity();
	}

	private void FixedUpdate()
	{
		HandleRunning();

		HandleSliding();
	}

	#region INPUT CALLBACKS
	private void SetUpPlayerInput()
	{
		if (_playerController == null)
		{
			_playerController = new PlayerController();

			_playerController.Player.Move.performed += _playerController => _moveInput = _playerController.ReadValue<Vector2>();
			_playerController.Player.Jump.performed += OnJumpInput;
			_playerController.Player.Jump.canceled += OnJumpUpInput;
			_playerController.Player.Dash.performed += OnDashInput;
		}
	}

	private void HandleInput()
    {
		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);
	}

	public void OnJumpInput(InputAction.CallbackContext context)
	{
		_lastTimePressedJump = _data.JumpInputBufferTime;
	}

	public void OnJumpUpInput(InputAction.CallbackContext context)
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}

	public void OnDashInput(InputAction.CallbackContext context)
	{
		_lastTimePressedDash = _data.DashInputBufferTime;
	}

	#endregion

	#region GENERAL METHODS

	private void HandleRunning()
    {
		if (!_isDashing)
		{
			if (_isWallJumping)
				Run(_data.WallJumpRunLerp);
			else
				Run(1);
		}
		else if (_isDashAttacking)
		{
			Run(_data.DashEndRunLerp);
		}
	}

	private void HandleSliding()
    {
		if (_isSliding)
			Slide();
	}

	private void ManageGravity()
    {
		if (!_isDashAttacking)
		{
			//Higher gravity if we've released the jump input or are falling
			if (_isSliding)
			{
				SetGravityScale(0);
			}
			else if (_body.velocity.y < 0 && _moveInput.y < 0)
			{
				//Much higher gravity if holding down
				SetGravityScale(_data.GravityScale * _data.FastFallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				_body.velocity = new Vector2(_body.velocity.x, Mathf.Max(_body.velocity.y, -_data.MaxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				//Higher gravity if jump button released
				SetGravityScale(_data.GravityScale * _data.JumpCutGravityMult);
				_body.velocity = new Vector2(_body.velocity.x, Mathf.Max(_body.velocity.y, -_data.MaxFallSpeed));
			}
			else if ((_isJumping || _isWallJumping || _isJumpFalling) && Mathf.Abs(_body.velocity.y) < _data.JumpHangTimeThreshold)
			{
				SetGravityScale(_data.GravityScale * _data.JumpHangGravityMult);
			}
			else if (_body.velocity.y < 0)
			{
				//Higher gravity if falling
				SetGravityScale(_data.GravityScale * _data.FallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				_body.velocity = new Vector2(_body.velocity.x, Mathf.Max(_body.velocity.y, -_data.MaxFallSpeed));
			}
			else
			{
				//Default gravity if standing on a platform or moving upwards
				SetGravityScale(_data.GravityScale);
			}
		}
		else
		{
			//No gravity when dashing (returns to normal once initial dashAttack phase over)
			SetGravityScale(0);
		}
	}

	private void CheckOnGround()
    {
		if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !_isJumping) //checks if set box overlaps with ground
		{
			if (_lastTimeOnGround < -0.1f)
			{
				_playerAnimator.JustLanded = true;
			}

			_lastTimeOnGround = _data.CoyoteTime; //if so sets the lastGrounded to coyoteTime
		}
	}

	private void CheckCollisions()
    {
		if (!_isDashing && !_isJumping)
		{
			CheckOnGround();

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && _isFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !_isFacingRight)) && !_isWallJumping)
				_lastTimeOnRightWall = _data.CoyoteTime;

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !_isFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && _isFacingRight)) && !_isWallJumping)
				_lastTimeOnLeftWall = _data.CoyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			_lastTimeOnWall = Mathf.Max(_lastTimeOnLeftWall, _lastTimeOnRightWall);
		}
	}

	public void SetGravityScale(float scale)
	{
		_body.gravityScale = scale;
	}

	private void HandleTimers(float delta)
    {
		_lastTimeOnGround -= delta;
		_lastTimeOnWall -= delta;
		_lastTimeOnRightWall -= delta;
		_lastTimeOnLeftWall -= delta;
		_lastTimePressedJump -= delta;
		_lastTimePressedDash -= delta;
	}

	private void Sleep(float duration)
	{
		StartCoroutine(PerformSleep(duration));
	}

	private IEnumerator PerformSleep(float duration)
	{
		Time.timeScale = 0;
		yield return Helpers.GetRealTimeWait(duration); //Must be Realtime since timeScale with be 0 
		Time.timeScale = 1;
	}
	
	#endregion

	//MOVEMENT METHODS
	#region RUN METHODS

	private float CalculateAccelerationRate(float targetSpeed)
    {
		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (_lastTimeOnGround > 0)
			return (Mathf.Abs(targetSpeed) > 0.01f) ? _data.RunAccelAmount : _data.RunDeccelAmount;
		else
			return (Mathf.Abs(targetSpeed) > 0.01f) ? _data.RunAccelAmount * _data.AccelInAir : _data.RunDeccelAmount * _data.DeccelInAir;
	}

	private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * _data.MaxRunSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(_body.velocity.x, targetSpeed, lerpAmount);

		float accelRate = CalculateAccelerationRate(targetSpeed);

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((_isJumping || _isWallJumping || _isJumpFalling) && Mathf.Abs(_body.velocity.y) < _data.JumpHangTimeThreshold)
		{
			accelRate *= _data.JumpHangAccelerationMult;
			targetSpeed *= _data.JumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if (_data.DoConserveMomentum && Mathf.Abs(_body.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(_body.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && _lastTimeOnGround < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0;
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - _body.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		_body.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;

		_isFacingRight = !_isFacingRight;
	}
	#endregion

	#region JUMP METHODS

	private void JumpChecks()
    {
		if (_isJumping && _body.velocity.y < 0f)
		{
			_isJumping = false;

			if (!_isWallJumping)
				_isJumpFalling = true;
		}

		if (_isWallJumping && Time.time - _wallJumpStartTime > _data.WallJumpTime)
		{
			_isWallJumping = false;
		}

		if (_lastTimeOnGround > 0f && !_isJumping && !_isWallJumping)
		{
			_isJumpCut = false;

			if (!_isJumping)
				_isJumpFalling = false;
		}

		if (!_isDashing)
		{
			//Jump
			if (CanJump() && _lastTimePressedJump > 0f)
			{
				_isJumping = true;
				_isWallJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				Jump();

				_playerAnimator.StartedJumping = true;
			}
			//WALL JUMP
			else if (CanWallJump() && _lastTimePressedJump > 0f)
			{
				_isWallJumping = true;
				_isJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (_lastTimeOnRightWall > 0f) ? -1 : 1;

				WallJump(_lastWallJumpDir);
			}
		}
	}

	public void Jump()
	{
		//Ensures we can't call Jump multiple times from one press
		_lastTimePressedJump = 0;
		_lastTimeOnGround = 0;

		PerformJump();
	}

	private void PerformJump()
    {
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		float force = _data.JumpForce;

		if (_body.velocity.y < 0)
			force -= _body.velocity.y;

		_body.AddForce(Vector2.up * force, ForceMode2D.Impulse);
	}

	private void WallJump(int dir)
	{
		//Ensures we can't call Wall Jump multiple times from one press
		_lastTimePressedJump = 0f;
		_lastTimeOnGround = 0f;
		_lastTimeOnRightWall = 0f;
		_lastTimeOnLeftWall = 0f;

		PerformWallJump(dir);
	}

	private void PerformWallJump(int dir)
    {
		Vector2 force = new Vector2(_data.WallJumpForce.x, _data.WallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(_body.velocity.x) != Mathf.Sign(force.x))
			force.x -= _body.velocity.x;

		if (_body.velocity.y < 0f) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= _body.velocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		_body.AddForce(force, ForceMode2D.Impulse);
	}

	#endregion

	#region DASH METHODS

	private void DashChecks()
    {
		if (CanDash() && _lastTimePressedDash > 0)
		{
			//Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			Sleep(_data.DashSleepTime);

			//If not direction pressed, dash forward
			if (_moveInput != Vector2.zero)
				_lastDashDir = _moveInput;
			else
				_lastDashDir = _isFacingRight ? Vector2.right : Vector2.left;



			_isDashing = true;
			_isJumping = false;
			_isWallJumping = false;
			_isJumpCut = false;

			StartCoroutine(StartDash(_lastDashDir));
		}
	}

    private IEnumerator StartDash(Vector2 dir)
	{
		//Overall this method of dashing aims to mimic Celeste, if you're looking for
		// a more physics-based approach try a method similar to that used in the jump

		_lastTimeOnGround = 0;
		_lastTimePressedDash = 0;

		float startTime = Time.time;

		_dashesLeft--;
		_isDashAttacking = true;

		SetGravityScale(0);

		//We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
		while (Time.time - startTime <= _data.DashAttackTime)
		{
			_body.velocity = dir.normalized * _data.DashSpeed;
			//Pauses the loop until the next frame, creating something of a Update loop. 
			//This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
			yield return null;
		}

		startTime = Time.time;

		_isDashAttacking = false;

		//Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
		SetGravityScale(_data.GravityScale);
		_body.velocity = _data.DashEndSpeed * dir.normalized;

		while (Time.time - startTime <= _data.DashEndTime)
		{
			yield return null;
		}

		//Dash over
		_isDashing = false;
	}

	//Short period before the player is able to dash again
	private IEnumerator RefillDash(int amount)
	{
		//SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
		_dashRefilling = true;
		yield return new WaitForSeconds(_data.DashRefillTime);
		_dashRefilling = false;
		_dashesLeft = Mathf.Min(_data.DashAmount, _dashesLeft + 1);
	}
	#endregion

	#region Slide

	private void SlideChecks()
    {
		if (CanSlide() && ((_lastTimeOnLeftWall > 0 && _moveInput.x < 0) || (_lastTimeOnRightWall > 0 && _moveInput.x > 0)))
			_isSliding = true;
		else
			_isSliding = false;
	}

	private void Slide()
	{
		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = _data.SlideSpeed - _body.velocity.y;
		float movement = speedDif * _data.SlideAccel;
		//So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
		//The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		_body.AddForce(movement * Vector2.up);
	}

	#endregion


	#region CHECK METHODS

	public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != _isFacingRight)
			Turn();
	}

	private bool CanJump()
	{
		return _lastTimeOnGround > 0 && !_isJumping;
	}

	private bool CanWallJump()
	{
		return _lastTimePressedJump > 0 && _lastTimeOnWall > 0 && _lastTimeOnGround <= 0 && (!_isWallJumping ||
			 (_lastTimeOnRightWall > 0 && _lastWallJumpDir == 1) || (_lastTimeOnLeftWall > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
	{
		return _isJumping && _body.velocity.y > 0;
	}

	private bool CanWallJumpCut()
	{
		return _isWallJumping && _body.velocity.y > 0;
	}

	private bool CanDash()
	{
		if (!_isDashing && _dashesLeft < _data.DashAmount && _lastTimeOnGround > 0 && !_dashRefilling)
		{
			StartCoroutine(RefillDash(1));
		}

		return _dashesLeft > 0;
	}

	public bool CanSlide()
	{
		return (_lastTimeOnWall > 0 && !_isJumping && !_isWallJumping && !_isDashing && _lastTimeOnGround <= 0);
	}
	#endregion


	#region EDITOR METHODS

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}

	#endregion
}