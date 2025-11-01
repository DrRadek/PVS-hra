using Godot;
using System;

public partial class Projectiĺe : CharacterBody2D
{
    public delegate int FunctionEventHandler(int x);
    FunctionEventHandler functionEvent;
    // TODO:
    //  projectile should follow a path of the given function
    //  it should react to IHittable and hit it with the calculated damage
    //  function can be invoked like this: int y = functionEvent.Invoke(x);
    //  how to move CharacterBody2D: https://docs.godotengine.org/en/stable/tutorials/physics/using_character_body_2d.html
    // Idea:
    //  a new class ProjectiveShooter will for example shoot x projectiles every y seconds
    //  All projectiles will follow the trajectory defined by the function and increment their x every move (based on given speed)

    public void SetFunction(FunctionEventHandler function)
    {
        this.functionEvent = function;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Move the projectile here
    }
}
