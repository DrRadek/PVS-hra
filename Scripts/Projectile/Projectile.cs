using Godot;
using System;

namespace PVShra.Projectile
{
    public partial class Projectile : Area2D
    {
        [Export] public float Speed = 300.0f;
        [Export] public float MaxDistance = 500.0f;
        [Export] public float Scale = 1.0f;
        
        public Vector2 Direction { get; set; }
        public Functions.FunctionType FunctionType { get; set; } = Functions.FunctionType.Sin;
        
        private float _distanceTraveled = 0.0f;
        private Vector2 _startPosition;
        private Functions.IFunction _function;
        
        public override void _Ready()
        {
            _startPosition = GlobalPosition;
            _function = Functions.FunctionFactory.CreateFunction(FunctionType);
            
            BodyEntered += OnBodyEntered;
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float movement = Speed * (float)delta;
            GlobalPosition += Direction * movement;
            _distanceTraveled += movement;
            
            if (_distanceTraveled >= MaxDistance)
            {
                QueueFree();
            }
        }
        
        private void OnBodyEntered(Node2D body)
        {
            if (body is Enemy.Enemy enemy)
            {
                float damage = CalculateDamage();
                enemy.TakeDamage(damage);
                QueueFree();
            }
        }
        
        private float CalculateDamage()
        {
            // Damage is based on f(x) where x is the distance traveled
            float x = _distanceTraveled / 100.0f; // Scale down for reasonable function values
            float functionValue = _function.Evaluate(x * Scale);
            
            // Ensure minimum damage of 1
            return Math.Max(1.0f, Math.Abs(functionValue) * 10.0f);
        }
    }
}
