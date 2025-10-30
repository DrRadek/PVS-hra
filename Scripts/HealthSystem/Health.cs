using Godot;
using System;

public partial class Health : AbstractHealth
{
    [Export] int health = 100;

    public override bool GetHitAbsolute(int amount)
    {
        // TODO: implement, should return whether it survived
        throw new NotImplementedException();
    }

    public override bool GetHitRelative(int amount)
    {
        // TODO: implement, should return whether it survived
        throw new NotImplementedException();
    }

    public override void HealAbsolute(int amount)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public override void HealRelative(int percentage)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
