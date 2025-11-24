using Godot;
using System;

public partial class Player : RigidBody2D, IHittable
{
    [Export] NodePath healthManagerLocation;
    [Export] NodePath functionsManagerLocation;
    [Export] public Node2D storageNode;
    AbstractHealthManager healthManager;
    FunctionsManager functionsManager;

    public void GetHit(float amount, bool isAbsolute)
    {
        healthManager.GetHit(amount, isAbsolute);
    }

    public override void _Ready()
    {
        healthManager = (AbstractHealthManager)GetNode(healthManagerLocation);
        functionsManager = (FunctionsManager)GetNode(functionsManagerLocation);

        healthManager.OnDeath += OnDeath;
    }

    void OnDeath()
    {
        // TODO: implement
        GD.Print("PLAYER DIED");
    }
}
