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
        Instance = this;

        RestartGame();
    }
}
