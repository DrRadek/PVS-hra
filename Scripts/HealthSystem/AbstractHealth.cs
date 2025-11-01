using Godot;
using System;

public abstract partial class AbstractHealth : Node
{
    // True = survived
    public abstract bool GetHit(float amount, bool isAbsolute = true);

    public abstract void Heal(float amount, bool isAbsolute = true);

}
