using Godot;
using System;

public partial class Enemy : Node
{
    [Export] TargetFollower targetFollower;

    // TODO: target 
    [Export] Node2D target;

    public override void _Ready()
    {
        base._Ready();
    }
}
