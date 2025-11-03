using Godot;
using System;

public partial class MovableObject : Node
{
    [Export] float speed = 200;
    [Export] RigidBody2D rb;


    public void Move(Vector2 direction)
    {
        if (rb == null) return;

        var dir = direction;
        if (dir.LengthSquared() > 1f) dir = dir.Normalized();

        rb.LinearVelocity = dir * speed;
        rb.Sleeping = false;
    }

    public Node2D GetBodyNode2D() => rb as Node2D;
    public Vector2 GetBodyGlobalPosition() => (rb as Node2D)?.GlobalPosition ?? Vector2.Zero;
}
