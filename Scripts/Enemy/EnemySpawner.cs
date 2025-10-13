using Godot;
using System;

namespace PVShra.Enemy
{
    public partial class EnemySpawner : Node2D
    {
        [Export] public float SpawnInterval = 2.0f;
        [Export] public float SpawnRadius = 600.0f;
        [Export] public float MinHealth = 5.0f;
        [Export] public float MaxHealth = 20.0f;
        
        private float _spawnTimer = 0.0f;
        private PackedScene _enemyScene;
        private Node2D _player;
        private Random _random = new Random();
        
        public override void _Ready()
        {
            _enemyScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
            _player = GetNode<Node2D>("/root/Main/Player");
        }
        
        public override void _Process(double delta)
        {
            _spawnTimer += (float)delta;
            
            if (_spawnTimer >= SpawnInterval)
            {
                SpawnEnemy();
                _spawnTimer = 0.0f;
            }
        }
        
        private void SpawnEnemy()
        {
            if (_enemyScene == null || _player == null) return;
            
            var enemy = _enemyScene.Instantiate<Enemy>();
            
            // Random angle around the player
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            Vector2 spawnOffset = new Vector2(
                (float)Math.Cos(angle) * SpawnRadius,
                (float)Math.Sin(angle) * SpawnRadius
            );
            
            enemy.GlobalPosition = _player.GlobalPosition + spawnOffset;
            
            // Random health (which is also the enemy's "number")
            float health = (float)(_random.NextDouble() * (MaxHealth - MinHealth) + MinHealth);
            enemy.Health = (float)Math.Round(health);
            
            GetParent().AddChild(enemy);
        }
    }
}
