using Godot;
using System;

public partial class Projectile : Area2D
{
    public delegate float FunctionEventHandler(float x);
    FunctionEventHandler damageFunction;
    FunctionEventHandler trajectoryFunction;

    float maxTimeAlive = 10;
    float time = 0;
    float distance = 0;
    float lastY = 0;

    float speed;
    int reverseMultiplier;

    public override void _Ready()
    {
        BodyEntered += Projectile_BodyEntered;
    }

    private void Projectile_BodyEntered(Node2D body)
    {
        if (body is IHittable hittable && body is Enemy)
        {
            hittable.GetHit(damageFunction(distance * reverseMultiplier * 0.01f), true);
            GetParent().QueueFree();
        }
    }

    public void Init(FunctionEventHandler damageFunction,
                         FunctionEventHandler trajectoryFunction,
                         float speed, bool reverse)
    {
        this.damageFunction = damageFunction;
        this.trajectoryFunction = trajectoryFunction;
        this.speed = speed;
        this.reverseMultiplier = reverse ? -1 : 1;

        this.lastY = trajectoryFunction(distance);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        time += dt;

        if (time > maxTimeAlive) {
            GetParent().QueueFree();
        }

        float lastX = distance;
        distance += (float)delta * speed;
        float y = trajectoryFunction(distance * reverseMultiplier * 0.01f);
        Position = new Vector2(distance * reverseMultiplier, -y * 100);
    }
}
