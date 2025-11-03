using Godot;
using System;

public partial class Player : RigidBody2D, IHittable
{
	[Export] NodePath healthManagerLocation;
	AbstractHealthManager healthManager;

	public void GetHit(float amount, bool isAbsolute)
	{
		healthManager.GetHit(amount, isAbsolute);
	}

	public override void _Ready()
	{
		healthManager = (AbstractHealthManager)GetNode(healthManagerLocation);
		healthManager.OnDeath += OnDeath;


	}

	void OnDeath()
	{
		// TODO: implement
	}
}
