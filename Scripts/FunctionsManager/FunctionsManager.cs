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

        var traj2 = new CosTrajectoryFunction();
        var dmg2 = new CosDamageFunction();

        unlockedTrajectoryFunctions.Add(traj);
        unlockedDamageFunctions.Add(dmg);

        unlockedTrajectoryFunctions.Add(traj2);
        unlockedDamageFunctions.Add(dmg2);

        if (projectileScenes.Count > 0)
        {
            var atk = new Attack(
                projectileScenes[0],
                traj,
                dmg,
                rotationNode
            );

            usedAttacks.Add(atk);

            var atk2 = new Attack(
                projectileScenes[0],
                traj2,
                dmg2,
                rotationNode
            );


            usedAttacks.Add(atk2);
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

    // Public accessors for PauseMenu and other UI
    public IReadOnlyList<TrajectoryFunction> GetUnlockedTrajectoryFunctions()
    {
        return unlockedTrajectoryFunctions.AsReadOnly();
    }

    public IReadOnlyList<DamageFunction> GetUnlockedDamageFunctions()
    {
        return unlockedDamageFunctions.AsReadOnly();
    }

    // Replace functions on a given attack slot (creates a new Attack instance preserving upgrade level)
    public void SetAttackFunctions(int attackIndex, TrajectoryFunction traj, DamageFunction dmg)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return;

        var old = usedAttacks[attackIndex];
        var newAtk = new Attack(projectileScenes.Count > 0 ? projectileScenes[0] : null, traj, dmg, rotationNode);
        newAtk.UpgradeLevel = old.UpgradeLevel;
        // preserve rotation offset from previous attack
        try { newAtk.RotationOffset = old.RotationOffset; } catch {}
        if (attacksEnabled) newAtk.Enable(); else newAtk.Disable();

        usedAttacks[attackIndex] = newAtk;
    }

    // Set rotation offset for an attack slot (radians)
    public void SetAttackRotation(int attackIndex, float rotation)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return;
        usedAttacks[attackIndex].RotationOffset = rotation;
    }

    public float GetAttackRotation(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return 0f;
        return usedAttacks[attackIndex].RotationOffset;
    }

    public void DisableAll()
    {
        attacksEnabled = false;

        foreach (var atk in usedAttacks)
            atk.Disable();
    }

    // expose number of attack slots
    public int GetAttackCount()
    {
        return usedAttacks.Count;
    }

    // expose functions assigned to an attack slot
    public TrajectoryFunction GetAttackTrajectory(int index)
    {
        if (index < 0 || index >= usedAttacks.Count) return null;
        return usedAttacks[index].Trajectory;
    }

    public DamageFunction GetAttackDamage(int index)
    {
        if (index < 0 || index >= usedAttacks.Count) return null;
        return usedAttacks[index].Damage;
    }

    public class UpgradableFunction
    {
        protected int upgradeLevel = 1;
        protected Projectile.FunctionEventHandler functionDefinition;
        protected string description = "f(x)";

        public string Description => description;

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
            description = "sin(x)";
        }
    }

    public class CosTrajectoryFunction : TrajectoryFunction
    {
        public CosTrajectoryFunction()
        {
            functionDefinition = (x) => Mathf.Cos(x) - 1;
            description = "cos(x) - 1";
        }
    }

    public class SinDamageFunction : DamageFunction
    {
        public SinDamageFunction()
        {
            functionDefinition = (x) => Mathf.Abs(Mathf.Sin(x)) * upgradeLevel + upgradeLevel;
            description = $"{upgradeLevel}+|sin(x)|*{upgradeLevel}";
        }
    }

    public class CosDamageFunction : DamageFunction
    {
        public CosDamageFunction()
        {
            functionDefinition = (x) => Mathf.Abs(Mathf.Cos(x)) * upgradeLevel + upgradeLevel;
            description = $"{upgradeLevel}+|cos(x)|*{upgradeLevel}";
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

        // rotation offset in radians applied to fired projectiles
        public float RotationOffset { get; set; } = 0f;

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

        // Expose assigned functions for UI inspection
        public TrajectoryFunction Trajectory => trajectoryFn;
        public DamageFunction Damage => damageFn;

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
            helperNode.GlobalRotation = angle + RotationOffset;
            helperNode.Scale = rotationNode.Scale;

            helperNode.AddChild(inst);
        }
    }
}
