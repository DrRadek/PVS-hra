using Godot;
using System;

public partial class Enemy : RigidBody2D
{
    [Export] TargetFollower targetFollower;

    [Export] Node2D target;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
    
    void OnBodyEntered(Node body)
    {
        if (body is IHittable)
        {
            // TODO: hit hittable if it's the target
        }
    }
}
