using Godot;
using System;

public abstract partial class AbstractHealthManager : Node
{
    [Signal] public delegate void OnDeathEventHandler();

    public abstract void GetHit(float amount, bool isAbsolute = true);

    public abstract void Heal(float amount, bool isAbsolute = true);
}
