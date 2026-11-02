using Godot;
using System;

public partial class HazardCollision : Area2D
{
	public override void _Ready()
	{
		// Listen for physics bodies entering this hazard area
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Check if the body entering the hazard is the Player
		if (body is Player player)
		{
			// Call the player's Die() method directly instead of reloading the scene
			player.Die();
		}
	}
}
