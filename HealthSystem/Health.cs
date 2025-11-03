using Godot;
using System;

public partial class Health : AbstractHealth
{
    [Export] int health = 100;

    public override bool GetHit(float amount, bool isAbsolute = true)
    {
        // TODO: implement, should return whether it survived
        // absolute = true => deal exactly the amount
        // absolute = false => deal damage in percentage
        throw new NotImplementedException();
    }

    public override void Heal(float amount, bool isAbsolute = true)
    {
        // TODO: implement
        // absolute = true => heal exactly the amount
        // absolute = false => heal percentage
        throw new NotImplementedException();
    }
}
