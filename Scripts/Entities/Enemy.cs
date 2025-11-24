using Godot;
using System;

public partial class Enemy : RigidBody2D, IHittable
{
    [Export] NodePath healthManagerLocation;
    [Export] TargetFollower targetFollower;
    [Export] PackedScene exp;
    [Export] int expDropAmount = 10; // TODO: move somewhere else?

    [Export] Node2D target;
    AbstractHealthManager healthManager;

    public override void _Ready()
    {
        healthManager = (AbstractHealthManager)GetNode(healthManagerLocation);
        BodyEntered += OnBodyEntered;

        healthManager.OnDeath += OnDeath;
    }

    public void GetHit(float amount, bool isAbsolute)
    {
        healthManager.GetHit(amount, isAbsolute);
    }

    void OnBodyEntered(Node body)
    {
        if (body is IHittable hittable && body is Player)
        {
            hittable.GetHit(1, true);
        }
    }

    void OnDeath()
    {
        Exp expNode = (Exp)exp.Instantiate();
        expNode.Init(expDropAmount);
        expNode.Position = GlobalPosition;
        GameManager.Instance.storageNode.CallDeferred("add_child", expNode);

        QueueFree();
    }
}
