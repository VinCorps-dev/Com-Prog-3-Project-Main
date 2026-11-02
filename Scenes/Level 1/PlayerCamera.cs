using Godot;

public partial class PlayerCamera : Camera2D
{
	[Export] public float FollowSpeed = 5.0f; // Pwede mong baguhin sa Inspector kung gusto mo mas mabagal o mabilis

	private Node2D _player;

	public override void _Ready()
	{
		// Kunin ang parent node (ang Player)
		_player = GetParent<Node2D>();
		
		// Gawing independent ang camera sa global space para hindi sumabay sa instant snap/teleport ng player
		TopLevel = true;
		
		if (_player != null)
		{
			GlobalPosition = _player.GlobalPosition;
		}
	}

	public override void _Process(double delta)
	{
		if (_player == null) return;

		// Smooth na hahabulin ng camera ang posisyon ng player kada frame.
		// Kahit maglakad, mag-dash, o mag-teleport (Die) ang player,
		// kusang mag-slide ang camera papunta sa kanya!
		GlobalPosition = GlobalPosition.Lerp(_player.GlobalPosition, (float)(FollowSpeed * delta));
	}
}
