using Godot;
using System;

public partial class MovableObject : Node
{
    [Export] float speed = 10;
    [Export] RigidBody2D rb;

    public void Move(Vector2 direction)
    {
        // TODO: implement moving of the rigidbody
    }
}
