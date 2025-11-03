using Godot;
using System;

public partial class TargetFollower : AbstractMovement
{
	[Export] public MovableObject movableObject; 
	[Export] public Node2D target;               
	[Export] public float stopDistance = 4f;

	public override void _Ready()
	{
		SetPhysicsProcess(true);

		if (movableObject == null)
			movableObject = GetNodeOrNull<MovableObject>("../MovableObject")
						 ?? GetNodeOrNull<MovableObject>("MovableObject");

		if (target == null)
			target = GetTree().GetFirstNodeInGroup("player") as Node2D
				  ?? GetTree().Root.FindChild("player", true, false) as Node2D
				  ?? GetTree().Root.FindChild("Player", true, false) as Node2D;

		GD.Print($"[TargetFollower] ready | mo={(movableObject!=null)} | target={(target!=null)}");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!enabled || movableObject == null || target == null) return;

		var myBody = movableObject.GetBodyNode2D();
		if (myBody == null) return;

		Vector2 toTarget = target.GlobalPosition - myBody.GlobalPosition;

		if (toTarget.LengthSquared() <= stopDistance * stopDistance)
		{
			movableObject.Move(Vector2.Zero);
			return;
		}

		movableObject.Move(toTarget.Normalized());
	}
}
