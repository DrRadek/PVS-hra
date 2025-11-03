using Godot;
using System;

public interface IHittable
{
    void GetHit(float damage, bool isAbsolute);
}
