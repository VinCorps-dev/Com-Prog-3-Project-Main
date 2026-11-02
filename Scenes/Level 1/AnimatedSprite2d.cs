using Godot;
using System;

public partial class AnimatedSprite2d : AnimatedSprite2D
{
	private Player _player;
	private bool _wasDashing = false;

	public override void _Ready()
	{
		// Cast parent to Player to access custom properties like dashing state
		_player = GetParent<Player>();
		Play("idle");
	}

	public override void _Process(double delta)
	{
		float direction = Input.GetAxis("left", "right");

		// 1. Flip the sprite left or right based on input direction
		if (direction > 0)
		{
			FlipH = false;
		}
		else if (direction < 0)
		{
			FlipH = true;
		}

		// 2. Switch animations based on player state
		if (_player != null)
		{
			// Dash animation takes top priority
			if (_player.IsDashing)
			{
				if (!_wasDashing)
				{
					Frame = 0;
					Play("dash");
					_wasDashing = true;
				}
			}
			else
			{
				_wasDashing = false;

				if (!_player.IsOnFloor())
				{
					// Check vertical velocity: falling if moving downwards, rising if jumping up
					if (_player.Velocity.Y > 0)
					{
						Play("falling"); // Updated to match your animation name
					}
					else
					{
						Play("jump");
					}
				}
				else if (direction != 0)
				{
					Play("run"); // Plays when moving on the ground
				}
				else
				{
					Play("idle"); // Plays when standing still
				}
			}
		}
	}
}
