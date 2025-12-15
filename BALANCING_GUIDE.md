# Quick Balancing Reference

## Current Default Values

### Player Stats
- **Starting HP**: 100
- **Starting Speed**: 200 (set in MovableObject)
- **Starting Function**: Constant (damage: 2, trajectory: straight line)

### Enemy Configuration
- **Score Reward**: 50 points per kill
- **XP Drop**: 10 XP per kill
- **Spawn Distance**: 500-1000 pixels from player
- **Map Bounds**: ±4500 pixels

### Progression
- **Level 1**: 100 XP required
- **Level 2**: 150 XP required
- **Level 3**: 200 XP required
- **Level N**: `100 + (N-1) * 50` XP required

### Difficulty Scaling
- **Formula**: `1.0 + (score / 1000.0)`
- **At 0 score**: 1.0x (100% difficulty)
- **At 1000 score**: 2.0x (200% difficulty)
- **At 5000 score**: 6.0x (600% difficulty)

### Upgrade Values
- **Health Upgrade**: +25 HP (heal to full)
- **Speed Upgrade**: +20% movement speed
- **Damage Upgrade**: +1 to damage function multiplier
- **Projectile Upgrade**: +1 to attack level (affects speed and fire rate)

## Recommended Tweaks for Different Difficulties

### Easy Mode
```csharp
// Enemy.cs
baseScoreReward = 75;      // More score
baseExpDropAmount = 15;    // More XP

// ScoreManager.cs - GetDifficultyMultiplier()
return 1.0f + (currentScore / 2000.0f);  // Slower difficulty ramp

// LevelManager.cs - GetRequiredXp()
return 75 + (level - 1) * 35;  // Faster leveling
```

### Hard Mode
```csharp
// Enemy.cs
baseScoreReward = 25;      // Less score
baseExpDropAmount = 5;     // Less XP

// ScoreManager.cs - GetDifficultyMultiplier()
return 1.0f + (currentScore / 500.0f);  // Faster difficulty ramp

// LevelManager.cs - GetRequiredXp()
return 150 + (level - 1) * 75;  // Slower leveling

// UpgradeMenu.cs - UpgradeMaxHealth
player.UpgradeMaxHealth(15);  // Less HP per upgrade
```

### Survival Mode (Long Games)
```csharp
// ScoreManager.cs - GetDifficultyMultiplier()
return 1.0f + Mathf.Log(currentScore / 100.0f + 1) * 0.5f;  // Logarithmic scaling

// Enemy.cs
baseExpDropAmount = 20;    // More XP for longer progression
```

## Function Damage Output Examples

At upgrade level 1, with x=1:
- **Constant**: 2 damage
- **Sin**: ~1.84 damage (1 + 0.84*1)
- **Cos**: ~1.54 damage (1 + 0.54*1)
- **Linear**: 1.5 damage (1 + 0.5*1)
- **Quadratic**: 1.2 damage (1 + 0.2*1)
- **Log**: ~1.69 damage (1 + 0.69*1)

At upgrade level 3, with x=1:
- **Constant**: 6 damage
- **Sin**: ~5.52 damage (3 + 0.84*3)
- **Cos**: ~4.62 damage (3 + 0.54*3)
- **Linear**: 4.5 damage (3 + 0.5*3)
- **Quadratic**: 3.6 damage (3 + 0.2*3)
- **Log**: ~5.07 damage (3 + 0.69*3)

## Score Milestones

With default settings (50 score per enemy):
- **20 kills**: 1000 score → 2x difficulty
- **40 kills**: 2000 score → 3x difficulty
- **60 kills**: 3000 score → 4x difficulty
- **100 kills**: 5000 score → 6x difficulty

## XP Milestones

With default settings (10 XP per enemy):
- **Level 2**: 10 kills (100 XP)
- **Level 3**: 15 more kills (150 XP) = 25 total
- **Level 4**: 20 more kills (200 XP) = 45 total
- **Level 5**: 25 more kills (250 XP) = 70 total

## Tips for Balancing

1. **Early Game Too Hard?**
   - Reduce enemy damage: Lower difficulty multiplier
   - Increase XP drops: Raise `baseExpDropAmount`
   - Start with more HP: Increase player's default `maxHealth`

2. **Late Game Too Easy?**
   - Increase difficulty scaling: Lower divisor in `GetDifficultyMultiplier()`
   - Reduce upgrade power: Lower health gain, speed multiplier
   - Increase XP requirements: Raise values in `GetRequiredXp()`

3. **Upgrades Feel Weak?**
   - Increase health gain: 25 → 50 HP
   - Increase speed boost: 20% → 30%
   - Make damage upgrades stronger: Modify function multipliers

4. **Too Many/Few Upgrades?**
   - Adjust `GetRequiredXp()` formula
   - Change `baseExpDropAmount` on enemies

5. **Functions Unbalanced?**
   - Adjust multipliers in function definitions
   - All functions should be viable choices
   - Consider context: Sin/Cos good for crowds, Linear for distance

## Playtesting Goals

Target for balanced game:
- First death around **level 5-7** (learning curve)
- Skilled players survive to **level 10+**
- Expert players reach **level 15-20**
- Upgrades feel meaningful and exciting
- No single "best" upgrade path
- Each function type has strategic value

## Quick Test Commands

Add these to your code temporarily for testing:

```csharp
// In Player._Process() or GameManager
if (Input.IsActionJustPressed("ui_page_up"))
{
    levelManager.AddXp(100); // Instant level up
}

if (Input.IsActionJustPressed("ui_page_down"))
{
    ScoreManager.Instance.AddScore(1000); // Instant score boost
}
```

Then test at different difficulty levels without grinding!
