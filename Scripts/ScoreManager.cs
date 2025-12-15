using Godot;
using System;

public partial class ScoreManager : Node
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    
    [Signal] public delegate void OnScoreChangedEventHandler(int newScore);

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.Print("ScoreManager: replacing existing Instance");
        }
        Instance = this;
        
        GD.Print("ScoreManager: Instance initialized");
        
        // Update UI on start - defer to ensure GameUI is ready
        CallDeferred(nameof(InitializeUI));
    }
    
    private void InitializeUI()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateScore(currentScore);
            GD.Print($"ScoreManager: Initial score {currentScore} sent to UI");
        }
        else
        {
            GD.PrintErr("ScoreManager: GameUI.Instance is null! Make sure GameUI exists in the scene.");
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        EmitSignal(SignalName.OnScoreChanged, currentScore);
        
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateScore(currentScore);
        }
        else
        {
            GD.Print("ScoreManager: GameUI.Instance is null, cannot update score display");
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        EmitSignal(SignalName.OnScoreChanged, currentScore);
        
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateScore(currentScore);
        }
    }

    // Get scaling factor based on score (for enemy difficulty)
    public float GetDifficultyMultiplier()
    {
        // Difficulty increases gradually with score
        // Every 100 score adds 10% difficulty
        return 1.0f + (currentScore / 1000.0f);
    }
}
