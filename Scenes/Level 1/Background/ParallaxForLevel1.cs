using Godot;
using System;

public partial class ParallaxForLevel1 : Node2D
{
	private Camera2D _camera;
	private Parallax2D[] _parallaxLayers;

	public override void _Ready()
	{
		// Hanapin ang lahat ng Parallax2D child nodes (Parallax 1 to 5)
		int childCount = GetChildCount();
		_parallaxLayers = new Parallax2D[childCount];

		for (int i = 0; i < childCount; i++)
		{
			if (GetChild(i) is Parallax2D layer)
			{
				_parallaxLayers[i] = layer;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (_camera == null)
		{
			_camera = GetViewport().GetCamera2D();
			return;
		}

		// Kung ang camera ay nasa kanan ng 0, hayaang gumalaw nang normal ang parallax.
		// Kung nasa 0 o mas kaliwa ang camera, i-lock ang scroll offset sa 0.
		if (_camera.GlobalPosition.X < 0)
		{
			foreach (var layer in _parallaxLayers)
			{
				if (layer != null)
				{
					// Pinipigilan ang background na mag-scroll papuntang negative X
					layer.ScreenOffset = new Vector2(0, layer.ScreenOffset.Y);
				}
			}
		}
	}
}
