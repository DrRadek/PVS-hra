using Godot;
using System;

public partial class MainMain : Node
{
    public static MainMain Instance { get; private set; }

    [Export] PackedScene mainScene;
    Node currentScene;
    public void RestartGame()
    {
        if (currentScene != null)
        {
            //RemoveChild(currentScene);
            GetTree().ReloadCurrentScene();
        }
        else
        {
            currentScene = mainScene.Instantiate();
            AddChild(currentScene);
        }
    }

    public override void _Ready()
    {
        if (Instance != null && Instance != this) GD.Print("MainMain: replacing existing Instance");
        Instance = this;

        RestartGame();
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }
}
