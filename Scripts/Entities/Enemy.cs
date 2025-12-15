using Godot;
using System;

public partial class Enemy : RigidBody2D, IHittable
{
    [Export] NodePath healthManagerLocation;
    [Export] TargetFollower targetFollower;
    [Export] PackedScene exp;
    [Export] int baseExpDropAmount = 10;
    [Export] int baseScoreReward = 50;

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
            // Scale damage with difficulty
            float damageMultiplier = ScoreManager.Instance != null ? ScoreManager.Instance.GetDifficultyMultiplier() : 1.0f;
            hittable.GetHit(1 * damageMultiplier, true);
        }
    }

    void OnDeath()
    {
        // Award score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(baseScoreReward);
        }

        // Drop XP
        Exp expNode = (Exp)exp.Instantiate();
        expNode.Init(baseExpDropAmount);
        expNode.Position = GlobalPosition;
        GameManager.Instance.storageNode.CallDeferred("add_child", expNode);

        QueueFree();
    }
}
