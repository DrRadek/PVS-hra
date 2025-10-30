using Godot;
using System;

public partial class Player : RigidBody2D
{
    [Export] NodePath healthManagerLocation;
    IHealthManager healthManager;

    public override void _Ready()
    {
        healthManager = (IHealthManager)GetNode(healthManagerLocation);
        healthManager.ConnecOnDeath(OnDeath2);
    }

    void OnDeath2()
    {
        // TODO: implement
    }
}
