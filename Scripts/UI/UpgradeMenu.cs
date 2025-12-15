using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UpgradeMenu : Control
{
    public static UpgradeMenu Instance { get; private set; }

    private VBoxContainer upgradeContainer;
    private List<UpgradeOption> currentOptions = new();
    
    private FunctionsManager functionsManager;
    private Player player;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.Print("UpgradeMenu: replacing existing Instance");
        }
        Instance = this;

        // CRITICAL: Allow this UI to work while game is paused
        ProcessMode = ProcessModeEnum.Always;

        // Create UI structure
        SetAnchorsPreset(LayoutPreset.FullRect);
        SetAnchor(Side.Left, 0);
        SetAnchor(Side.Top, 0);
        SetAnchor(Side.Right, 1);
        SetAnchor(Side.Bottom, 1);
        OffsetLeft = 0;
        OffsetTop = 0;
        OffsetRight = 0;
        OffsetBottom = 0;
        Visible = false;
        
        // Ensure we render on top
        ZIndex = 100;
        MouseFilter = MouseFilterEnum.Stop;

        // Semi-transparent background
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.8f);
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        bg.MouseFilter = MouseFilterEnum.Stop;
        AddChild(bg);

        // Center container
        var centerContainer = new CenterContainer();
        centerContainer.SetAnchorsPreset(LayoutPreset.FullRect);
        centerContainer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(centerContainer);

        // Panel
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(600, 400);
        centerContainer.AddChild(panel);

        // Main layout
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);

        // Title
        var title = new Label();
        title.Text = "LEVEL UP! Choose an Upgrade";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", Colors.Yellow);
        title.AddThemeFontSizeOverride("font_size", 32);
        vbox.AddChild(title);

        vbox.AddChild(new HSeparator());

        // Upgrades container
        upgradeContainer = new VBoxContainer();
        upgradeContainer.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(upgradeContainer);

        // Defer player/functionsManager lookup to next frame (player spawned by GameManager)
        CallDeferred(nameof(InitializeReferences));
    }
    
    private void InitializeReferences()
    {
        var playerNode = GetTree().GetFirstNodeInGroup("player") as Node;
        if (playerNode != null)
        {
            player = playerNode as Player;
            functionsManager = playerNode.GetNodeOrNull<FunctionsManager>("Scripts/FunctionsManager");
        }
        
        GD.Print($"UpgradeMenu: Initialized (player={player != null}, functionsManager={functionsManager != null})");
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowUpgradeOptions()
    {
        GD.Print("UpgradeMenu: ShowUpgradeOptions called");
        
        // Re-check references in case they weren't ready during _Ready
        if (functionsManager == null || player == null)
        {
            var playerNode = GetTree().GetFirstNodeInGroup("player") as Node;
            if (playerNode != null)
            {
                player = playerNode as Player;
                functionsManager = playerNode.GetNodeOrNull<FunctionsManager>("Scripts/FunctionsManager");
            }
        }
        
        if (functionsManager == null)
        {
            GD.PrintErr("UpgradeMenu: FunctionsManager not found");
            return;
        }
        
        if (player == null)
        {
            GD.PrintErr("UpgradeMenu: Player not found");
            return;
        }

        // Clear previous options
        foreach (var child in upgradeContainer.GetChildren())
        {
            child.QueueFree();
        }
        currentOptions.Clear();

        // Generate random upgrade options
        var possibleUpgrades = GeneratePossibleUpgrades();
        
        // Pick 3 random upgrades
        int optionCount = Mathf.Min(3, possibleUpgrades.Count);
        var selectedUpgrades = new List<UpgradeOption>();
        
        for (int i = 0; i < optionCount; i++)
        {
            if (possibleUpgrades.Count == 0) break;
            
            int randomIndex = GD.RandRange(0, possibleUpgrades.Count - 1);
            selectedUpgrades.Add(possibleUpgrades[randomIndex]);
            possibleUpgrades.RemoveAt(randomIndex);
        }

        // Display options
        foreach (var upgrade in selectedUpgrades)
        {
            CreateUpgradeButton(upgrade);
        }

        // Show menu and pause game
        Visible = true;
        GetTree().Paused = true;
        
        // Ensure we're on top of everything
        MoveToFront();
        
        GD.Print($"UpgradeMenu: Showing {selectedUpgrades.Count} upgrade options, game paused");
        GD.Print($"UpgradeMenu: Visible={Visible}, Position={GlobalPosition}, Size={Size}, Parent={GetParent()?.Name}");
    }

    private List<UpgradeOption> GeneratePossibleUpgrades()
    {
        var upgrades = new List<UpgradeOption>();

        if (functionsManager == null || player == null) return upgrades;

        // UNLIMITED UPGRADES - Always available
        
        // Health upgrade - unlimited
        upgrades.Add(new UpgradeOption
        {
            Name = "More Health",
            Description = "+25 Max HP and heal to full",
            IsLimited = false,
            Action = () => player?.UpgradeMaxHealth(25)
        });

        // Speed upgrade - unlimited
        upgrades.Add(new UpgradeOption
        {
            Name = "Faster Movement",
            Description = "+20% movement speed",
            IsLimited = false,
            Action = () => player?.UpgradeSpeed(0.2f)
        });

        // Passive regen upgrade - unlimited
        upgrades.Add(new UpgradeOption
        {
            Name = "Passive Regeneration",
            Description = "+2 HP per second regeneration",
            IsLimited = false,
            Action = () => player?.UpgradePassiveRegen(2.0f)
        });

        // Global damage upgrade - unlimited
        upgrades.Add(new UpgradeOption
        {
            Name = "Global Damage Boost",
            Description = "+20% damage to all attacks",
            IsLimited = false,
            Action = () => functionsManager?.UpgradeGlobalDamage(0.2f)
        });

        // LIMITED UPGRADES
        
        // Unlock new trajectory function (only if there are locked ones)
        var lockedTraj = functionsManager.GetLockedTrajectoryFunctions();
        if (lockedTraj != null && lockedTraj.Count > 0)
        {
            int randomTrajIndex = GD.RandRange(0, lockedTraj.Count - 1);
            if (randomTrajIndex < lockedTraj.Count)
            {
                var trajFunc = lockedTraj[randomTrajIndex];
                int capturedIndex = randomTrajIndex;
                upgrades.Add(new UpgradeOption
                {
                    Name = $"Unlock Trajectory: {trajFunc.Description}",
                    Description = "New projectile trajectory pattern",
                    IsLimited = true,
                    Action = () => functionsManager?.UnlockTrajectoryFunction(capturedIndex)
                });
            }
        }

        // Unlock new damage function (only if there are locked ones)
        var lockedDmg = functionsManager.GetLockedDamageFunctions();
        if (lockedDmg != null && lockedDmg.Count > 0)
        {
            int randomDmgIndex = GD.RandRange(0, lockedDmg.Count - 1);
            if (randomDmgIndex < lockedDmg.Count)
            {
                var dmgFunc = lockedDmg[randomDmgIndex];
                int capturedIndex = randomDmgIndex;
                upgrades.Add(new UpgradeOption
                {
                    Name = $"Unlock Damage: {dmgFunc.Description}",
                    Description = "New damage calculation formula",
                    IsLimited = true,
                    Action = () => functionsManager?.UnlockDamageFunction(capturedIndex)
                });
            }
        }

        // Upgrade damage for current attack (max 3 levels)
        int attackCount = functionsManager.GetAttackCount();
        if (attackCount > 0)
        {
            int attackIndex = GD.RandRange(0, attackCount - 1);
            int currentLevel = functionsManager.GetAttackDamageLevel(attackIndex);
            
            if (currentLevel < 3)
            {
                int capturedIndex = attackIndex;
                upgrades.Add(new UpgradeOption
                {
                    Name = $"Increase Damage (Lv {currentLevel}/3)",
                    Description = "Boost damage multiplier for your weapon",
                    IsLimited = true,
                    Action = () => functionsManager?.UpgradeAttackDamage(capturedIndex)
                });
            }
        }

        // Upgrade trajectory (speed) for current attack (max 3 levels)
        if (attackCount > 0)
        {
            int attackIndex2 = GD.RandRange(0, attackCount - 1);
            int currentLevel = functionsManager.GetAttackTrajectoryLevel(attackIndex2);
            
            if (currentLevel < 3)
            {
                int capturedIndex2 = attackIndex2;
                upgrades.Add(new UpgradeOption
                {
                    Name = $"Faster Projectiles (Lv {currentLevel}/3)",
                    Description = "Increase projectile speed and fire rate",
                    IsLimited = true,
                    Action = () => functionsManager?.UpgradeAttackTrajectory(capturedIndex2)
                });
            }
        }

        // Unlock backward shooting (one-time only)
        if (player != null && !player.CanShootBackwards())
        {
            upgrades.Add(new UpgradeOption
            {
                Name = "Shoot Backwards",
                Description = "Unlock the ability to fire behind you",
                IsLimited = true,
                Action = () => player?.UnlockBackwardShooting()
            });
        }

        return upgrades;
    }

    private void CreateUpgradeButton(UpgradeOption upgrade)
    {
        var button = new Button();
        button.ProcessMode = ProcessModeEnum.Always;
        button.MouseFilter = MouseFilterEnum.Stop;
        button.CustomMinimumSize = new Vector2(0, 80);
        
        var vbox = new VBoxContainer();
        button.AddChild(vbox);

        var nameLabel = new Label();
        nameLabel.Text = upgrade.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(nameLabel);

        var descLabel = new Label();
        descLabel.Text = upgrade.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        vbox.AddChild(descLabel);

        button.Pressed += () => OnUpgradeSelected(upgrade);
        
        upgradeContainer.AddChild(button);
        currentOptions.Add(upgrade);
    }

    private void OnUpgradeSelected(UpgradeOption upgrade)
    {
        // Execute the upgrade
        upgrade.Action?.Invoke();

        // Hide menu and unpause
        Visible = false;
        GetTree().Paused = false;

        GD.Print($"Upgrade selected: {upgrade.Name}");
    }

    private class UpgradeOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Action Action { get; set; }
        public bool IsLimited { get; set; } = false;
    }
}
