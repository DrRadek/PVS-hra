using Godot;
using System;
using static MovableObject;

public partial class LevelManager : Node
{
    int currentLevel = 1;
    int currentXp = 0;
    int xpRequired = 1;//100;

    [Signal] public delegate void OnLevelUpEventHandler();

    public override void _Ready()
    {
        GameUI.Instance.UpdateLevel(currentLevel, currentXp, xpRequired);
    }

    public void AddXp(int amount)
    {
        currentXp += amount;
        while (currentXp >= xpRequired)
        {
            currentXp -= xpRequired;
            currentLevel++;
            xpRequired = GetRequiredXp(currentLevel);
            EmitSignalOnLevelUp();
        }

        GameUI.Instance.UpdateLevel(currentLevel, currentXp, xpRequired);
    }

    int GetRequiredXp(int level)
    {
        //return xpRequired + (int)(40 * Math.Log(level));
        return xpRequired + level;
    }
}
