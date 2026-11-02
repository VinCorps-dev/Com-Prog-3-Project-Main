using Godot;

public partial class sawblade1 : Sprite2D
{
	[Export] public float SpinSpeed { get; set; } = 8.0f;

	public override void _Process(double delta)
	{
		Rotation += SpinSpeed * (float)delta;
	}
}
