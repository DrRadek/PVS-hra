using Godot;
using System;
using PVShra;
using PVShra.Projectile;

namespace PVShra.Player
{
    public partial class Player : CharacterBody2D
    {
        [Export] public float Speed = 200.0f;
        [Export] public float AttackCooldown = 0.5f;
        
        private float _attackTimer = 0.0f;
        private PackedScene _projectileScene;
        private GameManager _gameManager;
        
        public override void _Ready()
        {
            _projectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
            _gameManager = GetNode<GameManager>("/root/Main/GameManager");
        }
        
        public override void _PhysicsProcess(double delta)
        {
            HandleMovement();
            HandleAttack(delta);
            MoveAndSlide();
        }
        
        private void HandleMovement()
        {
            Vector2 velocity = Vector2.Zero;
            
            if (Input.IsActionPressed("move_up"))
                velocity.Y -= 1;
            if (Input.IsActionPressed("move_down"))
                velocity.Y += 1;
            if (Input.IsActionPressed("move_left"))
                velocity.X -= 1;
            if (Input.IsActionPressed("move_right"))
                velocity.X += 1;
            
            if (velocity.Length() > 0)
            {
                velocity = velocity.Normalized() * Speed;
            }
            
            Velocity = velocity;
        }
        
        private void HandleAttack(double delta)
        {
            _attackTimer -= (float)delta;
            
            if (_attackTimer <= 0)
            {
                Vector2 mousePos = GetGlobalMousePosition();
                Vector2 direction = (mousePos - GlobalPosition).Normalized();
                
                ShootProjectile(direction);
                _attackTimer = AttackCooldown;
            }
        }
        
        private void ShootProjectile(Vector2 direction)
        {
            if (_projectileScene == null || _gameManager == null) return;
            
            var projectile = _projectileScene.Instantiate<Projectile>();
            projectile.GlobalPosition = GlobalPosition;
            projectile.Direction = direction;
            projectile.FunctionType = _gameManager.CurrentFunction;
            
            GetParent().AddChild(projectile);
        }
    }
}
