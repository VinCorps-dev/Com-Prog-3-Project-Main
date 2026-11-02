using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement Settings")]
	[Export] public float Speed { get; set; } = 300.0f;
	[Export] public float JumpVelocity { get; set; } = -420.0f;
	[Export] public float JumpSpeedHorizontal { get; set; } = 300.0f;

	// Celeste Acceleration/Friction
	[Export] public float Acceleration { get; set; } = 1500.0f;
	[Export] public float Friction { get; set; } = 1500.0f;

	// Celeste Jump Tuning (Peak Float, Falling Gravity)
	[Export] public float GravityUp { get; set; } = 980.0f;
	[Export] public float GravityDown { get; set; } = 1600.0f;
	[Export] public float JumpApexThreshold { get; set; } = 40.0f;
	[Export] public float JumpApexGravityMult { get; set; } = 0.5f;

	// Fall Death, Position Clamping & Dynamic Respawn
	[ExportGroup("Level Boundaries & Death")]
	[Export] public float FallDeathY { get; set; } = 1000.0f; // Instantly dies if Y goes past this pixel value
	[Export] public bool LockToBoundaries { get; set; } = true;
	[Export] public float MinX { get; set; } = 0f;          // Left edge limit (in pixels)
	[Export] public float MaxX { get; set; } = 180000f;     // Right edge limit (in pixels)

	// Stores the active checkpoint location (defaults to starting position)
	public Vector2 RespawnPosition { get; set; }

	// Helper Timers for Game-Feel (Coyote & Buffering)
	[ExportGroup("Timers & Feels")]
	[Export] public float CoyoteTime { get; set; } = 0.1f;
	[Export] public float JumpBufferTime { get; set; } = 0.1f;

	private float _coyoteTimer = 0.0f;
	private float _jumpBufferTimer = 0.0f;

	// Jump & Dash configuration
	[ExportGroup("Dash Settings")]
	[Export] public int MaxJumps { get; set; } = 1;
	private int _jumpsRemaining;

	[Export] public float DashSpeed { get; set; } = 520.0f;
	[Export] public float DashDuration { get; set; } = 0.15f;
	[Export] public float DashEndSpeed { get; set; } = 350.0f;
	[Export] public float DashEndUpMultiplier { get; set; } = 0.75f;
	[Export] public float DashFreezeTime { get; set; } = 0.05f;
	[Export] public float DashCooldown { get; set; } = 0.2f;
	[Export] public float DashRefillDelay { get; set; } = 0.1f;

	private bool _canDash = true;
	private bool _isDashing = false;
	private float _dashTimer = 0.0f;
	private float _freezeTimer = 0.0f;
	private float _dashCooldownTimer = 0.0f;
	private float _dashRefillTimer = 0.0f;
	private Vector2 _dashDirection = Vector2.Zero;
	private float _lastFacingDirection = 1.0f;
	private Vector2 _preDashVelocity = Vector2.Zero;

	public bool IsDashing => _isDashing || _freezeTimer > 0;

	public override void _Ready()
	{
		// Set initial respawn point to wherever the player is placed in the scene
		RespawnPosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		// -------------------------------------------------------------
		// FALL DEATH CHECK
		// -------------------------------------------------------------
		if (GlobalPosition.Y > FallDeathY)
		{
			Die();
			return;
		}

		// Tick timers
		if (_dashCooldownTimer > 0) _dashCooldownTimer -= dt;
		if (_dashRefillTimer > 0) _dashRefillTimer -= dt;
		if (_coyoteTimer > 0) _coyoteTimer -= dt;
		if (_jumpBufferTimer > 0) _jumpBufferTimer -= dt;

		// Freeze frames: player is paused, then dash starts
		if (_freezeTimer > 0)
		{
			_freezeTimer -= dt;
			if (_freezeTimer <= 0)
			{
				BeginDash();
				MoveAndSlide();
				ClampHorizontalPosition();
			}
			return;
		}

		// Input direction
		float direction = Input.GetAxis("left", "right");
		if (direction != 0)
		{
			_lastFacingDirection = Mathf.Sign(direction);
		}

		// Update Jump Buffer
		if (Input.IsActionJustPressed("jump"))
		{
			_jumpBufferTimer = JumpBufferTime;
		}

		// Ground Check & Coyote Time Refills
		if (IsOnFloor() && !_isDashing)
		{
			_jumpsRemaining = MaxJumps;
			_coyoteTimer = CoyoteTime;

			if (_dashRefillTimer <= 0)
			{
				_canDash = true;
			}
		}

		// Active Dash
		if (_isDashing)
		{
			_dashTimer -= dt;

			if (_dashTimer > 0)
			{
				MoveAndSlide();
				ClampHorizontalPosition();
				return;
			}

			_isDashing = false;
			velocity = _dashDirection * DashEndSpeed;

			if (velocity.Y < 0)
			{
				velocity.Y *= DashEndUpMultiplier;
			}
		}

		// Trigger Dash
		if (Input.IsActionJustPressed("dash") && _canDash && _dashCooldownTimer <= 0)
		{
			_canDash = false;
			_freezeTimer = DashFreezeTime;
			_dashCooldownTimer = DashCooldown;
			_dashRefillTimer = DashRefillDelay;
			_preDashVelocity = velocity;
			return;
		}

		// Jump System
		bool isJumpingThisFrame = false;

		if (_jumpBufferTimer > 0)
		{
			if (_coyoteTimer > 0 || _jumpsRemaining > 0)
			{
				velocity = new Vector2(direction * JumpSpeedHorizontal, JumpVelocity);

				if (!IsOnFloor())
				{
					_jumpsRemaining--;
				}

				_jumpBufferTimer = 0.0f;
				_coyoteTimer = 0.0f;
				isJumpingThisFrame = true;
			}
		}

		// Variable Jump Cut
		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
		{
			velocity.Y *= 0.4f;
		}

		// Gravity & Apex Float
		if (!IsOnFloor())
		{
			float currentGravity = velocity.Y < 0 ? GravityUp : GravityDown;

			if (Mathf.Abs(velocity.Y) < JumpApexThreshold)
			{
				currentGravity *= JumpApexGravityMult;
			}

			velocity.Y += currentGravity * dt;
		}

		// Acceleration & Friction
		if (!isJumpingThisFrame)
		{
			if (direction != 0)
			{
				velocity.X = Mathf.MoveToward(velocity.X, direction * Speed, Acceleration * dt);
			}
			else
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * dt);
			}
		}

		Velocity = velocity;
		MoveAndSlide();

		// Keep player physically inside the horizontal limits of the level
		ClampHorizontalPosition();
	}

	/// <summary>
	/// Teleports the player to the current active checkpoint and resets movement state.
	/// </summary>
	public void Die()
	{
		GD.Print("DIED! Respawning at checkpoint: ", RespawnPosition);
		GetTree().CallGroup("FallingPlatforms", "ResetPlatform");
		Velocity = Vector2.Zero;
		_isDashing = false;
		_freezeTimer = 0.0f;
		_canDash = true;

		// Warp back to the latest checkpoint position
		GlobalPosition = RespawnPosition;
	}

	private void ClampHorizontalPosition()
	{
		if (!LockToBoundaries) return;

		Vector2 clampedPos = GlobalPosition;
		clampedPos.X = Mathf.Clamp(clampedPos.X, MinX, MaxX);
		GlobalPosition = clampedPos;
	}

	private void BeginDash()
	{
		Vector2 dashInput = GetDashInput();

		if (dashInput == Vector2.Zero)
		{
			dashInput = new Vector2(_lastFacingDirection, 0);
		}
		else
		{
			dashInput = SnapTo8Directions(dashInput);
		}

		_dashDirection = dashInput.Normalized();
		_isDashing = true;
		_dashTimer = DashDuration;

		float activeDashSpeed = DashSpeed;
		if (_dashDirection.Y == 0 && Mathf.Sign(_preDashVelocity.X) == Mathf.Sign(_dashDirection.X))
		{
			activeDashSpeed = Mathf.Max(DashSpeed, Mathf.Abs(_preDashVelocity.X));
		}

		Velocity = _dashDirection * activeDashSpeed;
	}

	private Vector2 GetDashInput()
	{
		float x = Input.GetAxis("left", "right");
		float y = 0.0f;

		if (InputMap.HasAction("up") && Input.IsActionPressed("up")) y -= 1.0f;
		if (InputMap.HasAction("down") && Input.IsActionPressed("down")) y += 1.0f;

		Vector2 input = new Vector2(x, y);
		if (input.Length() < 0.2f) return Vector2.Zero;
		return input;
	}

	private static Vector2 SnapTo8Directions(Vector2 input)
	{
		float step = Mathf.Pi / 4.0f;
		float snappedAngle = Mathf.Round(input.Angle() / step) * step;
		return Vector2.FromAngle(snappedAngle);
	}
}
