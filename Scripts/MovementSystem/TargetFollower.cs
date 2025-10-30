using Godot;
using System;

public partial class TargetFollower : AbstractMovement
{
    [Export] MovableObject movableObject;
    [Export] Node2D target;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (enabled)
        {
            // TODO: implement logic to move the object towards the target
        }
    }

    public void SetTarget(Node2D target)
    {
        this.target = target;
    }
}
