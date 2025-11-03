using Godot;
using System;

public partial class TargetFollower : AbstractMovement
{
    [Export] public MovableObject movableObject;
    [Export] public Node2D target;
    [Export] public float stopDistance = 0f;

    private RigidBody2D _rb;

    public void SetTarget(Node2D t) => target = t;

    public void SetMovable(MovableObject mo)
    {
        movableObject = mo;
        _rb = movableObject?.GetRigidBody2D();
    }

    public override void _Ready()
    {
        _rb = movableObject?.GetRigidBody2D();

        if (_rb == null)
            CallDeferred(nameof(InitCache));
    }

    private void InitCache()
    {
        if (_rb == null)
            _rb = movableObject?.GetRigidBody2D();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!enabled || _rb == null || target == null) return;

        Vector2 toTarget = target.GlobalPosition - _rb.GlobalPosition;

        if (toTarget.LengthSquared() <= stopDistance * stopDistance)
        {
            movableObject?.Move(Vector2.Zero);
            return;
        }

        movableObject?.Move(toTarget.Normalized());
    }
}
