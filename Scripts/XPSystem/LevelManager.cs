using Godot;
using System;

public partial class LevelManager : Node
{
    int currentLevel = 1;
    int currentXp = 0;
    int xpRequired = 100;

    [Signal] public delegate void OnLevelUpEventHandler();

    public override void _Ready()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateLevel(currentLevel, currentXp, xpRequired);
        }
    }

    public void AddXp(int amount)
    {
        currentXp += amount;
        while (currentXp >= xpRequired)
        {
            currentXp -= xpRequired;
            currentLevel++;
            xpRequired = GetRequiredXp(currentLevel);
            EmitSignal(SignalName.OnLevelUp);
        }

        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateLevel(currentLevel, currentXp, xpRequired);
        }
    }

    int GetRequiredXp(int level)
    {
        // Progressive scaling: 100 + (level * 50)
        // Level 1: 100, Level 2: 150, Level 3: 200, Level 4: 250, etc.
        return 100 + (level - 1) * 50;
    }
}
