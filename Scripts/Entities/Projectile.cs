using Godot;
using System;

public partial class Projectile : Area2D
{
    public delegate int FunctionEventHandler(int x);
    FunctionEventHandler functionEvent;
    // TODO:
    //  projectile should follow a path of the given function
    //  it should react to IHittable and hit it with the calculated damage
    //  function can be invoked like this: int y = functionEvent.Invoke(x);
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
        // Use this.Position to set position
        // Multiply by delta to make a framerate independent change of position (for example var changeX = speed * delta;)
        //  ChangeX then can be used to change the position
    }
}
