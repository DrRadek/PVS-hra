using Godot;
using System;

public partial class GameUI : Node
{
    [Export] Label levelLabel;
    [Export] TextureProgressBar levelProgressBar;
    [Export] Label hpLabel;
    [Export] TextureProgressBar hpProgressBar;
    [Export] Label scoreLabel;

    public static GameUI Instance { get; private set; }
    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.Print("GameUI: replacing existing Instance");
        }
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void UpdateLevel(int level, int currentXp, int xpRequired)
    {
        if (levelLabel != null)
        {
            levelLabel.Text = $"level {level} ({xpRequired - currentXp} xp left)";
        }
        if (levelProgressBar != null)
        {
            levelProgressBar.MaxValue = xpRequired;
            levelProgressBar.Value = currentXp;
        }
    }

    public void UpdateHp(float hp, float maxHp)
    {
        GD.Print("hp update");
        if (hpLabel != null)
        {
            hpLabel.Text = $"hp {hp}/{maxHp}";
        }
        if (hpProgressBar != null)
        {
            hpProgressBar.MaxValue = maxHp;
            hpProgressBar.Value = hp;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreLabel != null)
        {
            scoreLabel.Text = $"Score: {score}";
            GD.Print($"GameUI: Score updated to {score}");
        }
        else
        {
            GD.PrintErr("GameUI: scoreLabel is null! Did you add a ScoreLabel and assign it in the Inspector?");
        }
    }
}
