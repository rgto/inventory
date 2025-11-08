using Godot;
using System;

public partial class PlayerControllerMono : CharacterBody3D
{
	[Export] public Node3D CameraContainerNode { get; set; }
	[Export] public float VerticalRotationSpeed { get; set; } = 0.01f;
    [Export] public float HorizontalRotationSpeed { get; set; } = 0.5f;
	[Export] public AnimationPlayer AnimPlayer { get; set; }

	// --- NOVO: Adicione estas duas linhas no topo com as outras variáveis [Export] ---
	[Export] public Node3D CharacterModel { get; set; } // Arraste seu nó 'SK_Skin_4' aqui
	[Export] public float ModelRotationSpeed { get; set; } = 10.0f; // Velocidade da rotação do personagem

	[Export] public float WalkSpeed { get; set; } = 5.0f;
	[Export] public float RunSpeed { get; set; } = 8.0f;
	public const float JumpVelocity = 4.5f;

    public override void _Ready()
    {
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}

		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
            // --- MUDANÇA: A rotação do mouse agora afeta APENAS o corpo principal, não mais o modelo diretamente
			RotateY(Mathf.DegToRad(-motion.Relative.X) * HorizontalRotationSpeed);
			Vector3 cameraNodeRotation = CameraContainerNode.Rotation;
			cameraNodeRotation.X = Mathf.Clamp(cameraNodeRotation.X - motion.Relative.Y * VerticalRotationSpeed, Mathf.DegToRad(-20), Mathf.DegToRad(50));
			CameraContainerNode.Rotation = cameraNodeRotation;
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.MouseMode == Input.MouseModeEnum.Visible)
		{
            Velocity = velocity;
            MoveAndSlide();
            return;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		bool isSprinting = Input.IsActionPressed("sprint") && IsOnFloor();
		float currentSpeed = isSprinting ? RunSpeed : WalkSpeed;

		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
        // --- NOVO: Lógica para rotacionar o modelo do personagem ---
        if (CharacterModel != null && direction != Vector3.Zero)
        {
            // Usamos a direção do movimento para criar uma nova base de rotação que "olha" para essa direção.
            // O vetor Vector3.Up garante que o personagem não incline para cima ou para baixo.
            //var targetBasis = Basis.LookingAt(direction, Vector3.Up);
            var targetBasis = Basis.LookingAt(-direction, Vector3.Up);
            // Usamos Slerp (Spherical Linear Interpolation) para girar o modelo suavemente.
            // Isso evita uma rotação instantânea e "dura".
            CharacterModel.Basis = CharacterModel.Basis.Slerp(targetBasis, (float)(delta * ModelRotationSpeed));
        }

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, WalkSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, WalkSpeed);
		}

		HandleAnimations(direction, isSprinting);

		Velocity = velocity;
		MoveAndSlide();
	}

	private void HandleAnimations(Vector3 direction, bool isSprinting)
	{
		if (AnimPlayer == null)
		{
			return;
		}

		if (IsOnFloor())
		{
			if (direction != Vector3.Zero)
			{
				if (isSprinting)
				{
					if (AnimPlayer.CurrentAnimation != "Anim_ThirdPersonRun_6")
					{
						AnimPlayer.Play("Anim_ThirdPersonRun_6");
					}
				}
				else
				{
					if (AnimPlayer.CurrentAnimation != "Anim_ThirdPersonWalk_0")
					{
						AnimPlayer.Play("Anim_ThirdPersonWalk_0");
					}
				}
			}
			else
			{
				// --- MUDANÇA: Usei a animação de Idle que você colocou no código anterior ---
				if (AnimPlayer.CurrentAnimation != "Anim_ThirdPersonIdle_2")
				{
					AnimPlayer.Play("Anim_ThirdPersonIdle_2");
				}
			}
		}
	}
}