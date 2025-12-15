# Project Completion Guide

## Overview
This guide explains how to integrate all the new systems into your Godot game project.

## New Systems Added

### 1. Score System
- **File**: `Scripts/ScoreManager.cs`
- Tracks player score
- Provides difficulty multiplier based on score
- Integrates with GameUI

### 2. Upgrade System
- **File**: `Scripts/UI/UpgradeMenu.cs`
- Shows upgrade choices on level up
- Random selection of 3 upgrades from available pool
- Pauses game during selection

### 3. End Screen
- **File**: `Scripts/UI/EndScreen.cs`
- Displays final score on death
- Provides restart functionality

### 4. Expanded Function Library
- **Updated**: `Scripts/FunctionsManager/FunctionsManager.cs`
- **New Functions**:
  - Constant (starting function)
  - Linear (x)
  - Quadratic (x²)
  - Logarithmic (log(x))
  - Sin/Cos (existing, now unlockable)

## Scene Setup Instructions

### Step 1: Update main.tscn (or your main game scene)

1. **Add ScoreManager Node**:
   - Add a new Node as child of the root
   - Name it "ScoreManager"
   - Attach the `ScoreManager.cs` script to it

2. **Add UpgradeMenu Node**:
   - Add a new Control node as child of your UI layer (or root)
   - Name it "UpgradeMenu"
   - Attach the `UpgradeMenu.cs` script to it
   - Set its layout to "Full Rect"
   - The script will create all UI elements automatically

3. **Add EndScreen Node**:
   - Add a new Control node as child of your UI layer (or root)
   - Name it "EndScreen"
   - Attach the `EndScreen.cs` script to it
   - Set its layout to "Full Rect"
   - The script will create all UI elements automatically

### Step 2: Update GameUI

In your GameUI scene/node:
1. Add a new Label node for score display
2. Name it "ScoreLabel"
3. Position it where you want the score to appear (top-right is common)
4. In the GameUI node, set the exported property `scoreLabel` to reference this label

### Step 3: Update Player Scene (player.tscn)

1. **Find or add MovableObject**:
   - If your player has a MovableObject node, note its path
   - In Player script properties, set `movableObjectLocation` to this path
   
2. **Verify paths**:
   - Ensure `healthManagerLocation` points to your HealthManager
   - Ensure `functionsManagerLocation` points to your FunctionsManager
   - Ensure `levelManager` is assigned

### Step 4: Update Enemy Scene (enemy.tscn)

The enemy script now has these properties:
- `baseExpDropAmount` (default: 10) - XP dropped on death
- `baseScoreReward` (default: 50) - Score awarded on kill

Adjust these values in the inspector for balancing.

### Step 5: Update Health Component

If you have a Health node as a child of HealthManager:
- The script now supports `IncreaseMaxHealth()` method
- No scene changes needed

### Step 6: Update GameManager Settings

In your GameManager node, you can now set:
- `MapBounds` (default: 4500) - Maximum spawn distance from origin

## Balancing Configuration

### Enemy Rewards
Recommended starting values:
- **baseExpDropAmount**: 10-15 XP
- **baseScoreReward**: 50-100 points

### Difficulty Scaling
The difficulty multiplier increases by:
- 10% per 1000 score points
- Affects enemy speed and damage

### Player Starting Health
Recommended: 100 HP
- Each health upgrade adds 25 HP

### XP Requirements
Current formula in LevelManager:
```csharp
xpRequired = xpRequired + level;
```

For slower progression, change to:
```csharp
return xpRequired + (int)(40 * Math.Log(level));
```

### Function Balancing
All damage functions include upgrade multipliers:
- Constant: `2 * upgradeLevel`
- Sin/Cos: `upgradeLevel + |func(x)| * upgradeLevel`
- Linear: `upgradeLevel + |0.5x| * upgradeLevel`
- Quadratic: `upgradeLevel + 0.2x² * upgradeLevel`
- Logarithmic: `upgradeLevel + 2log(|x|+1) * upgradeLevel`

## Upgrade Types Available

1. **More Health**: +25 Max HP, heal to full
2. **Faster Movement**: +20% movement speed
3. **Unlock Trajectory Function**: Random locked trajectory
4. **Unlock Damage Function**: Random locked damage function
5. **Increase Damage**: Boost damage multiplier
6. **Faster Projectiles**: Increase speed and fire rate
7. **Shoot Backwards**: Enable backward firing (one-time unlock)

## Testing the Game

1. **Build and run** the game
2. Kill enemies to:
   - Gain score (shown in UI)
   - Collect XP
3. Level up to:
   - See upgrade menu
   - Choose an upgrade
4. Die to:
   - See end screen
   - View final score
   - Restart game

## Common Issues & Solutions

### UpgradeMenu doesn't show on level up
- Check that UpgradeMenu node exists in the scene
- Verify it's named "UpgradeMenu"
- Check console for "UpgradeMenu: FunctionsManager not found"

### Score doesn't display
- Verify GameUI has scoreLabel exported property set
- Check that ScoreManager node exists

### Enemies don't scale
- Ensure ScoreManager.Instance is accessible
- Check GameManager is applying difficulty multiplier

### End screen doesn't show
- Verify EndScreen node exists
- Check that Player.OnDeath calls EndScreen.ShowEndScreen()

### Player upgrades don't work
- Check movableObjectLocation is set in Player inspector
- Verify Health node path is correct

## File Structure Summary

```
Scripts/
├── ScoreManager.cs (NEW)
├── GameManager.cs (UPDATED)
├── Entities/
│   ├── Player.cs (UPDATED)
│   └── Enemy.cs (UPDATED)
├── FunctionsManager/
│   └── FunctionsManager.cs (UPDATED - added all function types)
├── HealthSystem/
│   └── Health.cs (UPDATED - added IncreaseMaxHealth)
├── MovementSystem/
│   └── MovableObject.cs (UPDATED - added speed multiplier)
└── UI/
    ├── GameUI.cs (UPDATED - added score)
    ├── UpgradeMenu.cs (NEW)
    └── EndScreen.cs (NEW)
```

## Next Steps for Further Development

1. **Add more upgrade types**:
   - Projectile piercing
   - Area of effect damage
   - Temporary invincibility
   - Score multipliers

2. **Improve balancing**:
   - Playtest and adjust XP/score rewards
   - Fine-tune difficulty scaling curve
   - Balance function damage values

3. **Visual enhancements**:
   - Add animations to upgrade menu
   - Create particle effects for level up
   - Improve end screen with statistics

4. **Save system**:
   - Track high scores
   - Unlock permanent upgrades
   - Achievement system

5. **Additional content**:
   - Boss enemies
   - Power-up drops
   - Different enemy types
   - Multiple player ships

## Debugging Tips

Enable debug output in console:
- Watch for "LEVEL UP - Showing upgrade menu"
- Check "Upgrade selected: [name]"
- Monitor "Score: X" updates
- Look for "PLAYER DIED" message

All systems use `GD.Print()` for important events, so keep the console open during testing.
