using Godot;
using System;

public partial class EndScreen : Control
{
    public static EndScreen Instance { get; private set; }

    private Label scoreLabel;
    private Label finalScoreLabel;
    private Button restartButton;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            GD.Print("EndScreen: replacing existing Instance");
        }
        Instance = this;

        // Create UI structure
        SetAnchorsPreset(LayoutPreset.FullRect);
        Visible = false;

        // Semi-transparent background
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.9f);
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Center container
        var centerContainer = new CenterContainer();
        centerContainer.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(centerContainer);

        // Main layout
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 30);
        centerContainer.AddChild(vbox);

        // Game Over title
        var title = new Label();
        title.Text = "GAME OVER";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", Colors.Red);
        title.AddThemeFontSizeOverride("font_size", 64);
        vbox.AddChild(title);

        // Final Score
        finalScoreLabel = new Label();
        finalScoreLabel.Text = "Final Score: 0";
        finalScoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
        finalScoreLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        finalScoreLabel.AddThemeFontSizeOverride("font_size", 48);
        vbox.AddChild(finalScoreLabel);

        vbox.AddChild(new HSeparator());

        // Restart button
        restartButton = new Button();
        restartButton.Text = "NEW GAME";
        restartButton.CustomMinimumSize = new Vector2(200, 60);
        restartButton.AddThemeFontSizeOverride("font_size", 24);
        
        var buttonContainer = new CenterContainer();
        buttonContainer.AddChild(restartButton);
        vbox.AddChild(buttonContainer);

        restartButton.Pressed += OnRestartPressed;
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowEndScreen()
    {
        // Get final score
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        finalScoreLabel.Text = $"Final Score: {finalScore}";

        // Show screen
        Visible = true;
        
        // Don't pause the tree here - let death animations play out
        // The player is already disabled
    }

    private void OnRestartPressed()
    {
        // Hide end screen
        Visible = false;
        
        // Unpause if paused
        GetTree().Paused = false;

        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Restart the game
        if (MainMain.Instance != null)
        {
            MainMain.Instance.RestartGame();
        }
    }
}
