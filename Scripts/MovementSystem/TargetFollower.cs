using Godot;
using System;

public partial class TargetFollower : AbstractMovement
{
    [Export] public MovableObject movableObject;
    [Export] public Node2D target;
    [Export] public float stopDistance = 0f;

    public void SetTarget(Node2D t) => target = t;

    public override void _PhysicsProcess(double delta)
    {
        if (!enabled || movableObject == null || target == null) return;

        var rb = movableObject.GetRigidBody2D();
        if (rb == null) return;

        Vector2 toTarget = target.GlobalPosition - rb.GlobalPosition;

        if (toTarget.LengthSquared() <= stopDistance * stopDistance)
        {
            movableObject.Move(Vector2.Zero);
            return;
        }

        movableObject.Move(toTarget.Normalized());
    }
}
