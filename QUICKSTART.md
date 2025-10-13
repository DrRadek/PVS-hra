# Quick Start Guide

## Prerequisites
- Godot 4.3+ with .NET support installed
- .NET 6.0 SDK installed

## Opening the Project
1. Launch Godot Engine
2. Click "Import" button
3. Navigate to this folder and select `project.godot`
4. Click "Import & Edit"
5. Wait for Godot to build the C# project (first time only)

## Running the Game
Press **F5** or click the **Play** button in the top-right corner

## Controls

### Movement
- **W** - Move Up
- **A** - Move Left  
- **S** - Move Down
- **D** - Move Right

### Combat
- **Mouse** - Aim projectiles (automatic shooting)
- **1-6** - Switch between unlocked functions

## Gameplay Tips

1. **Stay Mobile**: Keep moving to avoid getting surrounded by enemies

2. **Aim Wisely**: Your projectiles shoot towards the cursor, so position your mouse strategically

3. **Level Up**: Kill enemies to gain XP and unlock new mathematical functions
   - Level 2 at 50 XP → Unlock cos(x)
   - Level 3 at 150 XP → Unlock x (linear)
   - Level 4 at 300 XP → Unlock x² (quadratic)
   - Level 5 at 500 XP → Unlock tan(x)
   - Level 6 at 750 XP → Unlock e^(x/2) (exponential)

4. **Experiment with Functions**: Different functions deal different damage based on distance
   - **sin(x)** and **cos(x)**: Oscillating damage
   - **x**: Damage increases with distance
   - **x²**: Damage increases rapidly with distance
   - **tan(x)**: Variable damage with spikes
   - **e^(x/2)**: Exponential growth in damage

5. **Watch the Numbers**: Enemies display their health - prioritize weaker enemies (lower numbers)

## Understanding the UI

**Top Left Corner:**
```
Score: XXX          <- Total points earned
XP: XXX             <- Experience points
Level: X            <- Current level
Current Function: sin(x)  <- Active function (yellow)

Unlocked Functions:
  [1] sin(x)        <- Press 1 to use
  [2] cos(x)        <- Press 2 to use
  ...
```

## Troubleshooting

### Build Errors on First Open
- Ensure .NET 6.0 SDK is installed
- Close and reopen Godot
- Try clicking "Build" in the top menu

### Game Won't Start
- Check that `res://Scenes/Main.tscn` is set as the main scene in Project Settings
- Verify all scene files exist in the Scenes folder

### No Enemies Spawning
- Check that the EnemySpawner node exists in the Main scene
- Verify the Enemy.tscn file exists

### Projectiles Not Shooting
- Ensure the Projectile.tscn file exists
- Check that the Player has the correct script attached

## File Overview

- **project.godot** - Godot project configuration
- **PVS-hra.sln** - C# solution (for IDEs)
- **PVS-hra.csproj** - C# project configuration
- **Scripts/** - All game logic code
- **Scenes/** - All Godot scenes
- **README.md** - Detailed documentation
- **DESIGN.md** - Game design document
- **IMPLEMENTATION.md** - Implementation details

## Need Help?

Check the following files for more information:
- **README.md** - Full feature list and project structure
- **DESIGN.md** - Detailed game mechanics and architecture
- **IMPLEMENTATION.md** - Complete implementation summary

## Enjoy!

Have fun surviving against the numerical horde! 🎮🔢
