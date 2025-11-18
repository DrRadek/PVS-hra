using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class FunctionsManager : Node
{
    [Export] private Array<PackedScene> projectileScenes = new();

    [Export] private Node2D rotationNode;

    private List<Attack> usedAttacks = new();

    private List<TrajectoryFunction> unlockedTrajectoryFunctions = new();
    private List<DamageFunction> unlockedDamageFunctions = new();

    private List<TrajectoryFunction> lockedTrajectoryFunctions = new();
    private List<DamageFunction> lockedDamageFunctions = new();

    private bool attacksEnabled = false;

    MouseRotator mouseRotator;

    public override void _Ready()
    {
        mouseRotator = new MouseRotator(rotationNode);

        // Example reusable functions
        var traj = new SinTrajectoryFunction();
        var dmg = new SinDamageFunction();

        unlockedTrajectoryFunctions.Add(traj);
        unlockedDamageFunctions.Add(dmg);

        if (projectileScenes.Count > 0)
        {
            var atk = new Attack(
                projectileScenes[0],
                traj,
                dmg,
                rotationNode
            );

            usedAttacks.Add(atk);
        }

        EnableAll();
    }

    public override void _Process(double delta)
    {
        var rotation = mouseRotator.calculateRotation();

        if (Math.Abs(rotation) <= Mathf.Abs(Mathf.Pi/2))
        {
            rotationNode.Scale = new Vector2(1 * Mathf.Abs(rotationNode.Scale.X), rotationNode.Scale.Y);
        }
        else
        {
            rotationNode.Scale = new Vector2(-1 * Mathf.Abs(rotationNode.Scale.X), rotationNode.Scale.Y);
        }

        if (!attacksEnabled)
            return;

        float dt = (float)delta;
        foreach (var atk in usedAttacks)
            atk.Update(rotation, dt);
    }

    public void EnableAll()
    {
        attacksEnabled = true;

        foreach (var atk in usedAttacks)
        {
            atk.Enable();
        }
    }

    public void DisableAll()
    {
        attacksEnabled = false;

        foreach (var atk in usedAttacks)
            atk.Disable();
    }

    public class UpgradableFunction
    {
        protected int upgradeLevel = 1;
        protected Projectile.FunctionEventHandler functionDefinition;

        public int UpgradeLevel
        {
            get => upgradeLevel;
            set => upgradeLevel = value;
        }

        public Projectile.FunctionEventHandler FunctionDefinition => functionDefinition;
    }

    public class DamageFunction : UpgradableFunction { }

    public class TrajectoryFunction : UpgradableFunction
    {
        public float BaseSpeed = 200f;
        public float GetSpeed(int attackUpgrade) => BaseSpeed + attackUpgrade * 20f;
    }

    public class SinTrajectoryFunction : TrajectoryFunction
    {
        public SinTrajectoryFunction()
        {
            functionDefinition = (x) => Mathf.Sin(x);
        }
    }

    public class SinDamageFunction : DamageFunction
    {
        public SinDamageFunction()
        {
            functionDefinition = (x) => Mathf.Abs(Mathf.Sin(x)) * upgradeLevel + upgradeLevel;
        }
    }

    public class Attack
    {
        private readonly PackedScene projectileScene;
        private readonly TrajectoryFunction trajectoryFn;
        private readonly DamageFunction damageFn;

        private readonly Node2D rotationNode;

        // dynamic settings
        public int UpgradeLevel { get; set; } = 1;

        public float Cooldown => Mathf.Max(0.1f, 1f / UpgradeLevel);  // example
        public float BurstDelay => Mathf.Max(0.02f, 0.1f / UpgradeLevel);
        public int BurstCount => 1 + UpgradeLevel;

        public float ProjectileSpeed => trajectoryFn.GetSpeed(UpgradeLevel);

        private float cooldownTimer = 0f;
        private float shotTimer = 0f;
        private int shotsRemaining = 0;
        private bool enabled = false;

        public Attack(PackedScene scene,
                      TrajectoryFunction traj,
                      DamageFunction dmg,
                      Node2D rotationNode
            )
        {
            projectileScene = scene;
            trajectoryFn = traj;
            damageFn = dmg;
            this.rotationNode = rotationNode;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
            Reset();
        }

        void Reset()
        {
            cooldownTimer = 0f;
            shotTimer = 0f;
            shotsRemaining = 0;
        }

        public void Update(float angle, float delta)
        {
            if (!enabled)
                return;

            // cooldown
            if (cooldownTimer > 0f)
                cooldownTimer -= delta;

            // start burst
            if (cooldownTimer <= 0f && shotsRemaining == 0)
            {
                shotsRemaining = BurstCount;
                shotTimer = 0f; // fire immediately
                cooldownTimer = Cooldown;
            }

            // firing burst
            if (shotsRemaining > 0)
            {
                shotTimer -= delta;
                while (shotsRemaining > 0 && shotTimer <= 0f)
                {
                    FireProjectile(angle, true);
                    FireProjectile(angle, false);
                    shotsRemaining--;
                    shotTimer += BurstDelay;
                }
            }
        }

        private void FireProjectile(float angle, bool reverse = false)
        {
            var inst = projectileScene.Instantiate<Projectile>();
            float initialRot = rotationNode?.GlobalRotation ?? 0f;

            inst.Init(
                damageFn.FunctionDefinition,
                trajectoryFn.FunctionDefinition,
                ProjectileSpeed,
                reverse
            );

            var helperNode = new Node2D();
            GameManager.Instance.storageNode.AddChild(helperNode);
            helperNode.GlobalPosition = rotationNode.GlobalPosition;
            helperNode.GlobalRotation = angle;
            helperNode.Scale = rotationNode.Scale;

            helperNode.AddChild(inst);
        }
    }
}
