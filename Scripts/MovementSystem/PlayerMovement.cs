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
            // TODO: implement logic to move player based on inputs (call Move from the MovableObject)
            // Available moves: MoveRight, MoveLeft, MoveDown, MoveUp
            // Example: Input.IsActionPressed(MoveActions.MoveLeft);
            // Input.GetVector can also be used (see https://docs.godotengine.org/en/stable/tutorials/2d/2d_movement.html)
        }
    }
}
