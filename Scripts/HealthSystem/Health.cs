using Godot;
using System;

public partial class Health : AbstractHealth
{
    [Export] float maxHealth = 100;
    [Export] float health = 100;
    [Export] Node2D parentPosition;

    HealthPopup lastPopup;

    public override bool GetHit(float amount, bool isAbsolute = true)
    {
        // absolute = true => deal exactly the amount
        // absolute = false => deal damage in percentage
        float damageAmount = isAbsolute ? amount : maxHealth * amount;
        health -= damageAmount;

        if (!IsInstanceValid(lastPopup))
        {
            lastPopup = (HealthPopup)CommonScenes.healthPopupScene.Instantiate();
            GameManager.Instance.storageNode.AddChild(lastPopup);
        }

        lastPopup.GlobalPosition = parentPosition.GlobalPosition;
        lastPopup.Init(damageAmount, health, maxHealth);

        return health > 0;
    }

    public override void Heal(float amount, bool isAbsolute = true)
    {
        // absolute = true => heal exactly the amount
        // absolute = false => heal percentage
        health += isAbsolute ? amount : maxHealth * amount;
        return;
    }
}
