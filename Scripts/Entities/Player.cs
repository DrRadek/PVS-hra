using Godot;
using System;

public partial class Player : RigidBody2D, IHittable, IXpReceiver
{
    [Export] NodePath healthManagerLocation;
    [Export] NodePath functionsManagerLocation;
    [Export] LevelManager levelManager; 
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
        levelManager.OnLevelUp += OnLevelUp;
    }

    private void OnLevelUp()
    {
        GD.Print("LEVEL UP");
    }

    void OnDeath()
    {
        // TODO: implement
        GD.Print("PLAYER DIED");
    }

    public void ReceiveXp(int amount)
    {
        //GD.Print($"Player Collected {amount} xp");
        levelManager.AddXp(amount);
    }
}
