using Godot;
using System;

public interface IHealthManager
{
    void ConnecOnDeath(Action action);
}
