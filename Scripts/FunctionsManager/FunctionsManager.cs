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
    private float globalDamageMultiplier = 1.0f;

    MouseRotator mouseRotator;

    public override void _Ready()
    {
        // Ensure this node respects pause mode (don't process when paused)
        ProcessMode = ProcessModeEnum.Pausable;
        
        mouseRotator = new MouseRotator(rotationNode);

        // Initialize with constant functions (player starts basic)
        var constTraj = new ConstantTrajectoryFunction();
        var constDmg = new ConstantDamageFunction();

        unlockedTrajectoryFunctions.Add(constTraj);
        unlockedDamageFunctions.Add(constDmg);

        // Lock all advanced functions - they can be unlocked via upgrades
        lockedTrajectoryFunctions.Add(new SinTrajectoryFunction());
        lockedTrajectoryFunctions.Add(new CosTrajectoryFunction());
        lockedTrajectoryFunctions.Add(new LinearTrajectoryFunction());
        lockedTrajectoryFunctions.Add(new QuadraticTrajectoryFunction());
        lockedTrajectoryFunctions.Add(new LogarithmicTrajectoryFunction());

        lockedDamageFunctions.Add(new SinDamageFunction());
        lockedDamageFunctions.Add(new CosDamageFunction());
        lockedDamageFunctions.Add(new LinearDamageFunction());
        lockedDamageFunctions.Add(new QuadraticDamageFunction());
        lockedDamageFunctions.Add(new LogarithmicDamageFunction());

        if (projectileScenes.Count > 0)
        {
            var atk = new Attack(
                projectileScenes[0],
                constTraj,
                constDmg,
                rotationNode
            );

            usedAttacks.Add(atk);
        }

        EnableAll();
    }

    public override void _Process(double delta)
    {
        var rotation = mouseRotator.calculateRotation();

        // Normalize rotation to -90 to 90 degree range
        // Flip scale when aiming backwards
        if (Math.Abs(rotation) <= Mathf.Abs(Mathf.Pi/2))
        {
            rotationNode.Scale = new Vector2(1 * Mathf.Abs(rotationNode.Scale.X), rotationNode.Scale.Y);
        }
        else
        {
            rotationNode.Scale = new Vector2(-1 * Mathf.Abs(rotationNode.Scale.X), rotationNode.Scale.Y);
            // Normalize rotation to -90 to 90 range when facing left
            // Subtract π to bring angle back to right-facing equivalent
            // 180° becomes 0°, 135° becomes -45°, 225° becomes 45°
            if (rotation > Mathf.Pi/2)
                rotation = rotation - Mathf.Pi;
            else if (rotation < -Mathf.Pi/2)
                rotation = rotation + Mathf.Pi;
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

    public IReadOnlyList<TrajectoryFunction> GetLockedTrajectoryFunctions()
    {
        return lockedTrajectoryFunctions.AsReadOnly();
    }

    public IReadOnlyList<DamageFunction> GetLockedDamageFunctions()
    {
        return lockedDamageFunctions.AsReadOnly();
    }

    // Unlock a trajectory function from locked to unlocked
    public bool UnlockTrajectoryFunction(int lockedIndex)
    {
        if (lockedIndex < 0 || lockedIndex >= lockedTrajectoryFunctions.Count) return false;
        
        var func = lockedTrajectoryFunctions[lockedIndex];
        lockedTrajectoryFunctions.RemoveAt(lockedIndex);
        unlockedTrajectoryFunctions.Add(func);
        return true;
    }

    // Unlock a damage function from locked to unlocked
    public bool UnlockDamageFunction(int lockedIndex)
    {
        if (lockedIndex < 0 || lockedIndex >= lockedDamageFunctions.Count) return false;
        
        var func = lockedDamageFunctions[lockedIndex];
        lockedDamageFunctions.RemoveAt(lockedIndex);
        unlockedDamageFunctions.Add(func);
        return true;
    }

    // Upgrade a specific attack slot's damage (max level 3)
    public bool UpgradeAttackDamage(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return false;
        var attack = usedAttacks[attackIndex];
        if (attack.Damage != null && attack.Damage.UpgradeLevel < 3)
        {
            attack.Damage.UpgradeLevel++;
            return true;
        }
        return false;
    }

    // Upgrade a specific attack slot's trajectory/speed (max level 3)
    public bool UpgradeAttackTrajectory(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return false;
        var attack = usedAttacks[attackIndex];
        if (attack.UpgradeLevel < 3)
        {
            attack.UpgradeLevel++;
            return true;
        }
        return false;
    }

    // Get upgrade level for attack damage
    public int GetAttackDamageLevel(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return 0;
        return usedAttacks[attackIndex].Damage?.UpgradeLevel ?? 0;
    }

    // Get upgrade level for attack trajectory
    public int GetAttackTrajectoryLevel(int attackIndex)
    {
        if (attackIndex < 0 || attackIndex >= usedAttacks.Count) return 0;
        return usedAttacks[attackIndex].UpgradeLevel;
    }

    // Upgrade global damage multiplier (unlimited)
    public void UpgradeGlobalDamage(float amount)
    {
        globalDamageMultiplier += amount;
        GD.Print($"\"Global damage multiplier increased to {globalDamageMultiplier}x\")");
    }

    public float GetGlobalDamageMultiplier()
    {
        return globalDamageMultiplier;
    }

    // Add a new attack slot (max 5 total)
    public bool AddAttackSlot()
    {
        if (usedAttacks.Count >= 5)
        {
            GD.Print("FunctionsManager: Already at max attack slots (5)");
            return false;
        }

        if (projectileScenes.Count == 0)
        {
            GD.PrintErr("FunctionsManager: No projectile scenes available");
            return false;
        }

        // Use first unlocked functions
        var traj = unlockedTrajectoryFunctions.Count > 0 ? unlockedTrajectoryFunctions[0] : new ConstantTrajectoryFunction();
        var dmg = unlockedDamageFunctions.Count > 0 ? unlockedDamageFunctions[0] : new ConstantDamageFunction();

        var newAttack = new Attack(
            projectileScenes[0],
            traj,
            dmg,
            rotationNode
        );

        // Set rotation offset for variety
        float angleOffset = (Mathf.Tau / (usedAttacks.Count + 1)) * 0.3f;
        newAttack.RotationOffset = angleOffset * (usedAttacks.Count % 2 == 0 ? 1 : -1);

        usedAttacks.Add(newAttack);
        if (attacksEnabled) newAttack.Enable();

        GD.Print($"FunctionsManager: Added attack slot {usedAttacks.Count}/5");
        return true;
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

    public class ConstantDamageFunction : DamageFunction
    {
        public ConstantDamageFunction()
        {
            functionDefinition = (x) => 2.0f * upgradeLevel;
            description = $"2*{upgradeLevel}";
        }
    }

    public class LinearDamageFunction : DamageFunction
    {
        public LinearDamageFunction()
        {
            functionDefinition = (x) => Mathf.Abs(x * 0.5f) * upgradeLevel + upgradeLevel;
            description = $"{upgradeLevel}+|0.5x|*{upgradeLevel}";
        }
    }

    public class QuadraticDamageFunction : DamageFunction
    {
        public QuadraticDamageFunction()
        {
            functionDefinition = (x) => (x * x * 0.2f) * upgradeLevel + upgradeLevel;
            description = $"{upgradeLevel}+0.2x²*{upgradeLevel}";
        }
    }

    public class LogarithmicDamageFunction : DamageFunction
    {
        public LogarithmicDamageFunction()
        {
            functionDefinition = (x) => Mathf.Max(0, Mathf.Log(Mathf.Abs(x) + 1) * 2.0f) * upgradeLevel + upgradeLevel;
            description = $"{upgradeLevel}+2log(|x|+1)*{upgradeLevel}";
        }
    }

    public class ConstantTrajectoryFunction : TrajectoryFunction
    {
        public ConstantTrajectoryFunction()
        {
            functionDefinition = (x) => 0.0f;
            description = "0 (straight)";
        }
    }

    public class LinearTrajectoryFunction : TrajectoryFunction
    {
        public LinearTrajectoryFunction()
        {
            functionDefinition = (x) => x * 0.3f;
            description = "0.3x";
        }
    }

    public class QuadraticTrajectoryFunction : TrajectoryFunction
    {
        public QuadraticTrajectoryFunction()
        {
            functionDefinition = (x) => x * x * 0.1f;
            description = "0.1x²";
        }
    }

    public class LogarithmicTrajectoryFunction : TrajectoryFunction
    {
        public LogarithmicTrajectoryFunction()
        {
            functionDefinition = (x) => Mathf.Log(Mathf.Abs(x) + 1) * 0.5f;
            description = "0.5log(|x|+1)";
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
                    FireProjectile(angle, false);
                    // Only fire backward if unlocked
                    if (CanShootBackwards())
                    {
                        FireProjectile(angle, true);
                    }
                    shotsRemaining--;
                    shotTimer += BurstDelay;
                }
            }
        }

        private bool CanShootBackwards()
        {
            // Get player reference to check backward shooting ability
            var player = rotationNode?.GetTree()?.GetFirstNodeInGroup("player") as Player;
            return player != null && player.CanShootBackwards();
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
            // Copy scale - the negative X scale flips projectile direction when facing left
            helperNode.Scale = rotationNode.Scale;

            helperNode.AddChild(inst);
        }
    }
}
