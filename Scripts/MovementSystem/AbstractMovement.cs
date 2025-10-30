using Godot;
using System;

public abstract partial class AbstractMovement : Node
{
    public bool enabled = true;
    public void Enable() { enabled = true; }
    public void Disable() { enabled = false; }
}
