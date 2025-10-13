using Godot;
using System;
using PVShra;

namespace PVShra.Enemy
{
    public partial class Enemy : CharacterBody2D
    {
        [Export] public float Speed = 100.0f;
        [Export] public float Health { get; set; } = 10.0f;
        [Export] public int ScoreValue = 10;
        [Export] public int XpValue = 5;
        
        private Node2D _player;
        private Label _healthLabel;
        
        public override void _Ready()
        {
            _player = GetNode<Node2D>("/root/Main/Player");
            
            // Create health label showing the number
            _healthLabel = new Label();
            _healthLabel.Position = new Vector2(-15, -30);
            _healthLabel.AddThemeColorOverride("font_color", Colors.White);
            _healthLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            _healthLabel.AddThemeFontSizeOverride("font_size", 20);
            AddChild(_healthLabel);
            
            UpdateHealthDisplay();
        }
        
        public override void _PhysicsProcess(double delta)
        {
            if (_player == null) return;
            
            Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;
            MoveAndSlide();
        }
        
        public void TakeDamage(float damage)
        {
            Health -= damage;
            UpdateHealthDisplay();
            
            if (Health <= 0)
            {
                Die();
            }
        }
        
        private void UpdateHealthDisplay()
        {
            if (_healthLabel != null)
            {
                _healthLabel.Text = ((int)Math.Ceiling(Health)).ToString();
            }
        }
        
        private void Die()
        {
            var gameManager = GetNode<GameManager>("/root/Main/GameManager");
            if (gameManager != null)
            {
                gameManager.AddScore(ScoreValue);
                gameManager.AddXP(XpValue);
            }
            
            QueueFree();
        }
    }
}
