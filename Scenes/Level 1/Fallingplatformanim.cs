using Godot;

public partial class Fallingplatformanim : RigidBody2D
{
	[ExportGroup("Falling Settings")]
	[Export] public float FallDelay { get; set; } = 0.2f; // Oras bago tuluyang bumagsak

	private bool _isTriggered = false;
	private float _timer = 0.0f;

	private Area2D _detectorArea;
	private AnimatedSprite2D _animatedSprite;

	// Para maalala ang orihinal na pwesto
	private Transform2D _startTransform;

	public override void _Ready()
	{
		// I-save ang orihinal na pwesto bago ito mahulog
		_startTransform = GlobalTransform;

		// Para matawag ng Player kapag namatay (CallGroup)
		AddToGroup("FallingPlatforms");

		// Siguraduhing naka-freeze siya sa simula para magsilbing matibay na sahig
		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;

		_detectorArea = GetNode<Area2D>("DetectorArea");
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		_detectorArea.BodyEntered += OnBodyEntered;

		// I-play ang Idle animation at hayaan itong mag-loop tuloy-tuloy
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation("Idle"))
		{
			_animatedSprite.Play("Idle");
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		// Huwag pansinin ang sarili o anumang RigidBody2D para hindi ma-detect ang sariling platform
		if (body == this || body is RigidBody2D) return;

		// Suriin kung ito ay ang Player o CharacterBody2D
		if (!_isTriggered && (body is CharacterBody2D || body.Name == "Player"))
		{
			_isTriggered = true;
			_timer = FallDelay;

			GD.Print("SUCCESS: Tumapak ang Player! Babagsak na sa loob ng " + _timer + "s");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isTriggered) return;

		_timer -= (float)delta;

		if (_timer <= 0)
		{
			// I-unfreeze para kusang mahulog gamit ang gravity ng RigidBody2D!
			Freeze = false;

			// Patayin muna ang physics process para hindi na umulit ang check
			// (bubuksan ulit sa ResetPlatform)
			SetPhysicsProcess(false);
		}
	}

	// TAWAGIN ITO KAPAG NAMATAY / NAG-RESPAWN ANG PLAYER
	public void ResetPlatform()
	{
		// Deferred para ligtas kahit tinawag mula sa loob ng physics callback
		CallDeferred(nameof(DoReset));
	}

	private void DoReset()
	{
		// Ibalik sa frozen para hindi na mahulog
		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;

		// Patigilin ang anumang galaw
		LinearVelocity = Vector2.Zero;
		AngularVelocity = 0f;

		// Ibalik sa orihinal na pwesto
		GlobalTransform = _startTransform;

		// I-reset ang state
		_isTriggered = false;
		_timer = 0.0f;
		SetPhysicsProcess(true);
	}
}
