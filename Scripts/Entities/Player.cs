using Godot;
using System;

public partial class Player : RigidBody2D, IHittable, IXpReceiver
{
    [Export] NodePath healthManagerLocation;
    [Export] NodePath functionsManagerLocation;
    [Export] NodePath movableObjectLocation;
    [Export] LevelManager levelManager; 
    [Export] public Node2D storageNode;
    
    AbstractHealthManager healthManager;
    FunctionsManager functionsManager;
    MovableObject movableObject;
    
    private bool canShootBackwards = false;
    private float passiveRegenRate = 0f; // HP per second
    private float regenTimer = 0f;

    public void GetHit(float amount, bool isAbsolute)
    {
        healthManager.GetHit(amount, isAbsolute);
    }

    public override void _Ready()
    {
        healthManager = (AbstractHealthManager)GetNode(healthManagerLocation);
        functionsManager = (FunctionsManager)GetNode(functionsManagerLocation);
        
        if (movableObjectLocation != null && !movableObjectLocation.IsEmpty)
        {
            movableObject = (MovableObject)GetNode(movableObjectLocation);
        }

        healthManager.OnDeath += OnDeath;
        levelManager.OnLevelUp += OnLevelUp;
    }

    public override void _Process(double delta)
    {
        // Passive health regeneration
        if (passiveRegenRate > 0)
        {
            regenTimer += (float)delta;
            if (regenTimer >= 1.0f)
            {
                regenTimer = 0f;
                var health = FindHealthNode(healthManager);
                if (health != null)
                {
                    health.Heal(passiveRegenRate, true);
                }
            }
        }
    }

    private void OnLevelUp()
    {
        GD.Print("LEVEL UP - Showing upgrade menu");
        
        // Show upgrade menu
        if (UpgradeMenu.Instance != null)
        {
            UpgradeMenu.Instance.ShowUpgradeOptions();
        }
        else
        {
            GD.PrintErr("Player: UpgradeMenu.Instance is null! Make sure UpgradeMenu node exists in the scene.");
        }
    }

    void OnDeath()
    {
        GD.Print("PLAYER DIED");
        
        // Show end screen
        if (EndScreen.Instance != null)
        {
            EndScreen.Instance.ShowEndScreen();
        }
        
        // Hide player
        Visible = false;
        
        // Disable player controls
        SetPhysicsProcess(false);
        SetProcess(false);
        
        if (functionsManager != null)
        {
            functionsManager.DisableAll();
        }
    }

    public void ReceiveXp(int amount)
    {
        GD.Print($"Player Collected {amount} xp");
        levelManager.AddXp(amount);
    }

    // Upgrade methods
    public void UpgradeMaxHealth(float amount)
    {
        // Try to find Health component through HealthManager
        if (healthManager != null)
        {
            var healthNode = FindHealthNode(healthManager);
            if (healthNode != null)
            {
                healthNode.IncreaseMaxHealth(amount);
                GD.Print($"Max health increased by {amount}");
            }
            else
            {
                GD.PrintErr("Could not find Health node for upgrade");
            }
        }
    }

    private Health FindHealthNode(Node startNode)
    {
        // Check if the node itself is Health
        if (startNode is Health h) return h;
        
        // Search children recursively
        foreach (Node child in startNode.GetChildren())
        {
            if (child is Health health) return health;
            var found = FindHealthNode(child);
            if (found != null) return found;
        }
        
        return null;
    }

    public void UpgradeSpeed(float multiplier)
    {
        if (movableObject != null)
        {
            float currentSpeed = movableObject.GetSpeed();
            movableObject.SetSpeedMultiplier(1.0f + multiplier);
            GD.Print($"Speed increased by {multiplier * 100}%");
        }
    }

    public void UnlockBackwardShooting()
    {
        canShootBackwards = true;
        GD.Print("Backward shooting unlocked!");
    }

    public bool CanShootBackwards()
    {
        return canShootBackwards;
    }

    public void UpgradePassiveRegen(float amount)
    {
        passiveRegenRate += amount;
        GD.Print($"Passive regen increased by {amount} HP/s (total: {passiveRegenRate} HP/s)");
    }

    public float GetPassiveRegen()
    {
        return passiveRegenRate;
    }
}
