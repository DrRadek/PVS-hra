using Godot;
using System;

public partial class MovableObject : Node
{
    [Flags]
    public enum CurrentDirection
    {
        None,
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8
    }

    CurrentDirection lastDirection = CurrentDirection.None;

    [Export] float speed = 200;
    [Export] RigidBody2D rb;


    [Signal] public delegate void OnCurrentDirChangedEventHandler(CurrentDirection currentDirection);

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

    Vector2 MapDirEnumToVector(CurrentDirection dir)
    {
        Vector2 finalDir = Vector2.Zero;
        finalDir.X = (((dir & CurrentDirection.Left) != 0) ? -1 : 0) + (((dir & CurrentDirection.Right) != 0) ? 1 : 0);
        finalDir.Y = (((dir & CurrentDirection.Up) != 0) ? -1 : 0) + (((dir & CurrentDirection.Down) != 0) ? 1 : 0);
        return finalDir.Normalized();
    }

    float dirEpsilon = 0.01f;

    public void Move(Vector2 direction)
    {
        var normalizedDir = direction.Normalized();

        CurrentDirection currentDirAnim = (
            (normalizedDir.X > dirEpsilon ? CurrentDirection.Right : (normalizedDir.X < -dirEpsilon ? CurrentDirection.Left : CurrentDirection.None)) |
            (normalizedDir.Y > dirEpsilon ? CurrentDirection.Down : (normalizedDir.Y < -dirEpsilon ? CurrentDirection.Up : CurrentDirection.None))
        );

        CurrentDirection currentDir = (
            (normalizedDir.X > 0 ? CurrentDirection.Right : (normalizedDir.X < 0 ? CurrentDirection.Left : CurrentDirection.None)) |
            (normalizedDir.Y > 0 ? CurrentDirection.Down : (normalizedDir.Y < 0 ? CurrentDirection.Up : CurrentDirection.None))
        );

        if (lastDirection != currentDirAnim)
            EmitSignalOnCurrentDirChanged(currentDirAnim);
        lastDirection = currentDirAnim;

        if (rb == null) return;
        var dir = MapDirEnumToVector(currentDir);
        if (currentDirAnim != CurrentDirection.None)
        {
            //rb.LinearVelocity = dir * speed;
            rb.ApplyForce(dir * speed);
        }
    }

    public RigidBody2D GetRigidBody2D() => rb;
}
