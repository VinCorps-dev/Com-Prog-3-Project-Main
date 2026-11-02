using Godot;

public partial class Checkpoint : Area2D
{
	private bool _isActivated = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print("Something entered checkpoint: ", body.Name);

		if (!_isActivated && body is Player player)
		{
			// Update the player's respawn position to this checkpoint's coordinates
			player.RespawnPosition = GlobalPosition;
			_isActivated = true;

			// Optional: Trigger an activation animation or visual change here
			// GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("activated");

			GD.Print("SUCCESS: Respawn position set to: ", GlobalPosition);
		}
	}
}
