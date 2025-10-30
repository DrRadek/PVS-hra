using Godot;
using System;

public partial class HealthManager : Node, IHealthManager
{
    [Signal] public delegate void OnDeathSignalEventHandler();

    public void ConnecOnDeath(Action action)
    {
        Connect(SignalName.OnDeathSignal, Callable.From(action));
    }

    void TriggerDeath()
    {
        EmitSignal(SignalName.OnDeathSignal);
    }
}
