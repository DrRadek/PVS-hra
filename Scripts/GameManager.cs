using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    // Singleton
    public static GameManager Instance { get; private set; }
    
    [Export] public PackedScene PlayerScene;
    [Export] public PackedScene EnemyScene;
    [Export] public float EnemyInterval = 1.5f;
    [Export] public int   EnemyMaxAlive = 10;
    [Export] public float EnemySpawnMinDistance = 500f; 
    [Export] public float EnemySpawnMaxDistance = 1000f;
    [Export] BackgroundMover backgroundMover;

    private Node2D _player;                 
    private readonly List<Node> _alive = new();

    public Node2D storageNode;

    public Node2D GetPlayer()
    {
        return _player;
    }

    public override void _Ready()
    {
        if (Instance == null)
            Instance = this;

        storageNode = this;

        _player = FindExistingPlayer() ?? SpawnPlayer();
        backgroundMover.SetPlayer(_player);
        SpawnLoop();
    }

    // ——— SPAWN PLAYER —————————————————————————————————————————————
    private Node2D SpawnPlayer()
    {
        if (PlayerScene == null)
        {
            GD.PushError("[GameManager] PlayerScene není nastavená.");
            return null;
        }
        var p = PlayerScene.Instantiate<Node2D>();
        (p as Player).storageNode = this;

        GetTree().CurrentScene.AddChild(p);
        p.GlobalPosition = Vector2.Zero;
        if (!p.IsInGroup("player")) p.AddToGroup("player");
        return p;
    }

    private Node2D FindExistingPlayer()
    {
        return GetTree().GetFirstNodeInGroup("player") as Node2D
            ?? GetTree().Root.FindChild("player", true, false) as Node2D
            ?? GetTree().Root.FindChild("Player", true, false) as Node2D;
    }

    // ——— SPAWN ENEMIES ———————————————————————————————————————————
    private async void SpawnLoop()
    {
        while (true)
        {
            _alive.RemoveAll(n => !IsInstanceValid(n));

            if (_player != null && EnemyScene != null && _alive.Count < EnemyMaxAlive)
            {
                var enemy = EnemyScene.Instantiate<Node2D>();
                enemy.GlobalPosition = GetEnemySpawnPosition();
                var follower = FindChildRecursive<TargetFollower>(enemy);
                if (follower != null) {
                    follower.SetTarget(_player);
                    var movable = FindChildRecursive<MovableObject>(enemy);
                    if (movable != null) follower.SetMovable(movable);
                }

                GetTree().CurrentScene.AddChild(enemy);
                _alive.Add(enemy);
                enemy.TreeExited += () => _alive.Remove(enemy);
            }

            await ToSignal(GetTree().CreateTimer(EnemyInterval), "timeout");
        }
    }

    private Vector2 GetEnemySpawnPosition()
    {
        if (_player == null) return Vector2.Zero;

        float a = GD.Randf() * Mathf.Tau;
        float d = Mathf.Lerp(EnemySpawnMinDistance, EnemySpawnMaxDistance, GD.Randf());
        return _player.GlobalPosition + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d;
    }

    private T FindChildRecursive<T>(Node root) where T : class
    {
        if (root is T t) return t;
        foreach (var c in root.GetChildren())
        {
            var n = c as Node;
            var f = FindChildRecursive<T>(n);
            if (f != null) return f;
        }
        return null;
    }
}
