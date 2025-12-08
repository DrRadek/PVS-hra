using Godot;
using System;

public partial class GameUI : Node
{
    [Export] Label levelLabel;
    [Export] TextureProgressBar levelProgressBar;
    [Export] Label hpLabel;
    [Export] TextureProgressBar hpProgressBar;

    public static GameUI Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;
    }

    public void UpdateLevel(int level, int currentXp, int xpRequired)
    {
        levelLabel.Text = $"level {level} ({xpRequired - currentXp} xp left)";
        levelProgressBar.MaxValue = xpRequired;
        levelProgressBar.Value = currentXp;
    }

    public void UpdateHp(float hp, float maxHp)
    {
        hpLabel.Text = $"hp {hp}/{maxHp}";
        hpProgressBar.MaxValue = maxHp;
        hpProgressBar.Value = hp;
    }
}
