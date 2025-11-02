using Godot;
using System;

public partial class PlayerMovement : AbstractMovement
{
	[Export] MovableObject movableObject;

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (!enabled || movableObject == null) return;

		var dir = Input.GetVector(
			MoveActions.MoveLeft,
			MoveActions.MoveRight,
			MoveActions.MoveUp,
			MoveActions.MoveDown
		);

		movableObject.Move(dir);
	}
}
