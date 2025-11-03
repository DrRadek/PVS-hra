using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnEnemy : Node 
{
	[Export] public float Interval { get; set; } = 1.5f;
	[Export] public int MaxAlive { get; set; } = 10;
	[Export] public string EnemyScenePath { get; set; } = "res://Scenes/enemy.tscn";
	[Export] public float MinSpawnDistance { get; set; } = 600f;   // minimální vzdálenost od hráče
	[Export] public float MaxSpawnDistance { get; set; } = 1200f;  // maximální vzdálenost 


	private readonly List<Node> _alive = new();
	private PackedScene _enemyScene;
	private Node2D _player;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D
			   ?? GetTree().Root.FindChild("player", true, false) as Node2D
			   ?? GetTree().Root.FindChild("Player", true, false) as Node2D;

		_enemyScene = ResourceLoader.Load<PackedScene>(EnemyScenePath);
		SpawnLoop();
	}

	private async void SpawnLoop()
	{
		while (true)
		{
			_alive.RemoveAll(n => !IsInstanceValid(n));

			if (_enemyScene != null && _player != null && _alive.Count < MaxAlive)
			{
				var enemy = _enemyScene.Instantiate() as Node2D;
				if (enemy != null)
				{
					enemy.GlobalPosition = RandomInAnnulus(_player.GlobalPosition, MinSpawnDistance, MaxSpawnDistance);
					WireEnemy(enemy, _player);
					GetTree().CurrentScene.AddChild(enemy);
					_alive.Add(enemy);
					enemy.TreeExited += () => _alive.Remove(enemy);
				}
			}

			await ToSignal(GetTree().CreateTimer(Interval), "timeout");
		}
	}

	private Vector2 RandomInAnnulus(Vector2 center, float minR, float maxR)
	{
		if (maxR < minR) (minR, maxR) = (maxR, minR);        
		if (Mathf.IsZeroApprox(maxR)) return center;

		// když je min ~ max, ber rovnou max
		if (Mathf.Abs(maxR - minR) < 0.001f) minR = maxR;

		float a = GD.Randf() * Mathf.Tau;
		float t = GD.Randf();
		float r = Mathf.Sqrt(Mathf.Lerp(minR * minR, maxR * maxR, t));
		return center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
	}


	private void WireEnemy(Node enemyRoot, Node2D player)
	{
		var follower = FindChildRecursive<TargetFollower>(enemyRoot);
		var movable  = FindChildRecursive<MovableObject>(enemyRoot);
		if (follower != null)
		{
			if (follower.target == null) follower.target = player;
			if (follower.movableObject == null) follower.movableObject = movable;
		}
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
