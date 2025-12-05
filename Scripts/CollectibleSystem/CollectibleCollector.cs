using Godot;
using System;
using System.Collections.Generic;

public partial class CollectibleCollector : Node
{
    [Export] Node2D collectorOwner;
    [Export] Area2D area;

    float pullStrength = 0.001f;
    [Export] float pullDistanceSquared = 100000;
    [Export] float collectDistanceSquared = 10000;

    HashSet<Node2D> collectibles = new();

    public override void _Ready()
    {
        area.AreaEntered += Area_AreaEntered;
        area.AreaExited += Area_AreaExited;
    }

    private void Area_AreaExited(Area2D area)
    {
        if (area is ICollectible)
        {
            collectibles.Remove(area);
        }
    }

    private void Area_AreaEntered(Area2D area)
    {
        if (area is ICollectible)
        {
            collectibles.Add(area);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach(var collectible in collectibles)
        {
            var distanceVector = (collectible.Position - collectorOwner.Position);
            var lengthSquared = distanceVector.LengthSquared();
            if (lengthSquared <= collectDistanceSquared)
            {
                (collectible as ICollectible).Collect(collectorOwner);
            }
            else
            {
                collectible.Position -= (distanceVector.Normalized()) * (Math.Max(pullDistanceSquared - lengthSquared, 0)) * pullStrength * (float)delta;
            }
        }
    }
}
