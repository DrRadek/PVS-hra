using Godot;
using System;

public partial class HealthManager : AbstractHealthManager
{
    // Note: here we can add more health types (shield, ...)
    [Export] AbstractHealth health;

    public override void GetHit(float amount, bool absolute = true)
    {
        if (!health.GetHit(amount, absolute))
        {
            EmitSignalOnDeath();
        }
    }

    public override void Heal(float amount, bool absolute = true)
    {
        health.Heal(amount, absolute);
    }
}
