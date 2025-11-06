using Godot;
using System;

public partial class Ui : Control
{

	public override void _Ready()
	{
		// Inventário começa fechado
		Visible = false;
	}

	public override void _Process(double delta)
	{
		// Alterna visibilidade com a tecla "I"
		if (Input.IsActionJustPressed("inventory_toggle"))
		{
			Visible = !Visible;
		}
	}
}
