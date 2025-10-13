# PVS-hra

A Vampire Survivors-inspired game built with Godot 4.5 and C#.

## Features

- **2D Top-Down View**: Classic top-down perspective
- **Player Movement**: Use WASD to move around
- **Function-Based Attacks**: 
  - Shoot projectiles in the direction of your cursor
  - Attacks use mathematical functions (sin, cos, tan, linear, quadratic, exponential)
  - Damage is calculated based on f(x) where x is the distance traveled
- **Number-Based Enemies**: 
  - Enemies spawn randomly and display their HP as numbers
  - Enemies move towards the player
- **Progression System**:
  - Gain score and XP by killing enemies
  - Level up to unlock new functions
  - Switch between unlocked functions using number keys (1-6)

## How to Play

1. Open the project in Godot 4.5 (C# version)
2. Run the Main scene
3. Use WASD to move your player (blue square)
4. Projectiles automatically shoot towards your cursor
5. Kill enemies (red squares with numbers) to gain score and XP
6. Level up to unlock new mathematical functions
7. Press 1-6 to switch between unlocked functions

## Controls

- **W/A/S/D**: Move player
- **Mouse**: Aim direction for projectiles
- **1-6**: Switch between unlocked functions

## Level Progression

- Level 2: Unlock at 50 XP
- Level 3: Unlock at 150 XP
- Level 4: Unlock at 300 XP
- Level 5: Unlock at 500 XP
- Level 6: Unlock at 750 XP
- Level 7: Unlock at 1000 XP

## Project Structure

```
Scripts/
├── Player/
│   └── Player.cs          # Player movement and shooting
├── Enemy/
│   ├── Enemy.cs           # Enemy behavior and health
│   └── EnemySpawner.cs    # Random enemy spawning
├── Projectile/
│   └── Projectile.cs      # Projectile behavior and damage
├── Functions/
│   └── FunctionSystem.cs  # Mathematical function implementations
└── Systems/
    ├── GameManager.cs     # Score, XP, and progression
    └── UI.cs              # User interface display

Scenes/
├── Main.tscn              # Main game scene
├── Player.tscn            # Player scene
├── Enemy.tscn             # Enemy scene
├── Projectile.tscn        # Projectile scene
└── EnemySpawner.tscn      # Enemy spawner scene
```

## Technical Details

- **Engine**: Godot 4.5 (C# only)
- **Language**: C# (.NET 6.0)
- **Architecture**: Modular design with separated concerns
- **Physics**: Uses Godot's built-in 2D physics system
- **Collision Layers**: Player (1), Enemy (2), Projectile (3)