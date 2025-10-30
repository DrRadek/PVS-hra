using Godot;
using System;

public partial class PlayerMovement : AbstractMovement
{
    [Export] MovableObject movableObject;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (enabled)
        {
            // TODO: implement logic to move player based on inputs
        }
    }
}
