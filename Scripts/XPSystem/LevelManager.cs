using Godot;
using System;
using static MovableObject;

public partial class LevelManager : Node
{
    int currentLevel = 1;
    int currentXp = 0;
    int xpRequired = 100;

    [Signal] public delegate void OnLevelUpEventHandler();

    public void AddXp(int amount)
    {
        currentXp += amount;
        if (currentXp >= xpRequired)
        {
            currentXp -= xpRequired;
            currentLevel++;
            xpRequired = GetRequiredXp(currentLevel);
            EmitSignalOnLevelUp();
        }
    }

    int GetRequiredXp(int level)
    {
        return xpRequired + (int)(40 * Math.Log(level));
    }
}
