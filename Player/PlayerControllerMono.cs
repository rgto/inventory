using Godot;
using System;

public partial class PlayerControllerMono : CharacterBody3D
{
	[Export] public Node3D CameraContainerNode { get; set; }
	[Export] public float VerticalRotationSpeed { get; set; } = 0.01f;
    [Export] public float HorizontalRotationSpeed { get; set; } = 0.5f;
	[Export] public AnimationPlayer AnimPlayer { get; set; }

    // --- MUDANÇA: Substituímos a velocidade constante por velocidades configuráveis ---
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

		// --- NOVO: Lógica de Corrida ---
		// Verifica se o jogador está pressionando a tecla de sprint e se está no chão
		bool isSprinting = Input.IsActionPressed("sprint") && IsOnFloor();
		float currentSpeed = isSprinting ? RunSpeed : WalkSpeed;

		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		if (direction != Vector3.Zero)
		{
			// --- MUDANÇA: Usa a velocidade atual (caminhada ou corrida) ---
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			// --- MUDANÇA: Usa a velocidade de caminhada para desacelerar ---
			velocity.X = Mathf.MoveToward(Velocity.X, 0, WalkSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, WalkSpeed);
		}

		// --- MUDANÇA: Passa a informação de "sprint" para o método de animação ---
		HandleAnimations(direction, isSprinting);

		Velocity = velocity;
		MoveAndSlide();
	}

	// --- MUDANÇA: O método agora também recebe se o jogador está correndo ---
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
				// --- NOVO: Lógica para escolher entre andar e correr ---
				if (isSprinting)
				{
					// Se está correndo, toca a animação de corrida
					if (AnimPlayer.CurrentAnimation != "Anim_ThirdPersonRun_6")
					{
						AnimPlayer.Play("Anim_ThirdPersonRun_6");
					}
				}
				else
				{
					// Se não está correndo, toca a animação de caminhada
					if (AnimPlayer.CurrentAnimation != "Anim_ThirdPersonWalk_0")
					{
						AnimPlayer.Play("Anim_ThirdPersonWalk_0");
					}
				}
			}
			else
			{
				AnimPlayer.Play("Anim_ThirdPersonIdle_2");
			}
		}
		// ... (lógica para animação de pulo pode entrar aqui)
	}
}