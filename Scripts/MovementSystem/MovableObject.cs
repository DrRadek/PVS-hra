using Godot;
using System;

public partial class MovableObject : Node
{
	[Export] float speed = 200;
	[Export] RigidBody2D rb;  // může zůstat prázdné – najdeme si ho sami

	public override void _Ready()
	{
		if (rb == null)
		{
			Node n = this;
			while (n != null && rb == null)
			{
				n = n.GetParent();
				if (n is RigidBody2D r) rb = r;
			}
		}
	}

	public void Move(Vector2 direction)
	{
		if (rb == null) return;

		var dir = direction;
		if (dir.LengthSquared() > 1f) dir = dir.Normalized();

		rb.LinearVelocity = dir * speed;
		rb.Sleeping = false; // probuď tělo, kdyby spalo
	}

	public Node2D GetBodyNode2D() => rb as Node2D;
	public Vector2 GetBodyGlobalPosition() => (rb as Node2D)?.GlobalPosition ?? Vector2.Zero;
}
