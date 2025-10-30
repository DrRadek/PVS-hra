using Godot;
using System;

public abstract partial class AbstractHealth : Node
{
    // True = survived
    public abstract bool GetHitAbsolute(int amount);
    // True = survived
    public abstract bool GetHitRelative(int amount);
    public abstract void HealAbsolute(int amount);
    public abstract void HealRelative(int percentage);

}
