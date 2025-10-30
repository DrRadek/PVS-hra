using Godot;
using System;

public partial class GameManager : Node
{
    // Singleton
    public static GameManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
