using Godot;
using System;

public partial class Camera2d : Camera2D
{
	[ExportGroup("Targeting")]
	[Export] public NodePath PlayerPath { get; set; } // Fixed: removed extra semicolon

	private Node2D _player;

	[ExportGroup("Level Limits")]
	[Export] public bool EnableLimits { get; set; } = true;
	[Export] public int LimitLeftPixel { get; set; } = 0;
	[Export] public int LimitRightPixel { get; set; } = 1920;
	[Export] public int LimitTopPixel { get; set; } = 0;
	[Export] public int LimitBottomPixel { get; set; } = 1080;

	public override void _Ready()
	{
		// Set Camera Limits
		if (EnableLimits)
		{
			LimitLeft = LimitLeftPixel;
			LimitRight = LimitRightPixel;
			LimitTop = LimitTopPixel;
			LimitBottom = LimitBottomPixel;
		}

		// Find Player Node
		if (PlayerPath != null && !PlayerPath.IsEmpty)
		{
			_player = GetNodeOrNull<Node2D>(PlayerPath);
		}
		else
		{
			// Fallback: use parent node if camera is attached as a child of Player
			_player = GetParent() as Node2D;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Smoothly follow player if camera is placed outside the Player node in the scene tree
		if (_player != null && GetParent() != _player)
		{
			GlobalPosition = _player.GlobalPosition;
		}
	}
}
