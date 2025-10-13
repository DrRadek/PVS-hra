# Game Design Document - PVS-hra

## Overview
A Vampire Survivors-inspired game with mathematical function-based combat.

## Core Mechanics

### Player
- **Movement**: WASD keys for 8-directional movement
- **Speed**: 200 units/second (configurable)
- **Appearance**: Blue square (24x24 pixels)
- **Collision Layer**: Layer 1 (Player)

### Combat System
- **Auto-attacking**: Projectiles shoot automatically towards cursor every 0.5 seconds
- **Function-based damage**: Each projectile uses a mathematical function
- **Damage calculation**: `damage = max(1, |f(x)| * 10)` where x = distance_traveled / 100
- **Projectile properties**:
  - Speed: 300 units/second
  - Max distance: 500 units
  - Scale: 1.0 (affects function calculation)

### Available Functions
1. **sin(x)** - Sine wave (unlocked by default)
2. **cos(x)** - Cosine wave (Level 2)
3. **x** - Linear (Level 3)
4. **x²** - Quadratic (Level 4)
5. **tan(x)** - Tangent (Level 5)
6. **e^(x/2)** - Exponential (Level 6)

### Enemies
- **Appearance**: Red squares (20x20 pixels) with health number displayed
- **Health**: Random between 5 and 20
- **Movement**: Move towards player at 100 units/second
- **Spawn**: Random position around player at 600 unit radius every 2 seconds
- **Rewards**:
  - Score: 10 points per kill
  - XP: 5 points per kill
- **Collision Layer**: Layer 2 (Enemy)

### Progression System
- **XP Thresholds**:
  - Level 2: 50 XP
  - Level 3: 150 XP
  - Level 4: 300 XP
  - Level 5: 500 XP
  - Level 6: 750 XP
  - Level 7: 1000 XP
- **Level ups**: Unlock new mathematical functions
- **Function switching**: Press number keys 1-6 to switch between unlocked functions

### UI Display
- Score (top-left)
- XP (below score)
- Level (below XP)
- Current function (highlighted in yellow)
- List of unlocked functions with key bindings

## Technical Architecture

### Class Structure

```
PVShra/
├── Functions/
│   ├── IFunction (interface)
│   ├── SinFunction
│   ├── CosFunction
│   ├── TanFunction
│   ├── LinearFunction
│   ├── QuadraticFunction
│   ├── ExponentialFunction
│   └── FunctionFactory (static)
│
├── Player/
│   └── Player : CharacterBody2D
│       - HandleMovement()
│       - HandleAttack()
│       - ShootProjectile()
│
├── Enemy/
│   ├── Enemy : CharacterBody2D
│   │   - TakeDamage()
│   │   - Die()
│   │   - UpdateHealthDisplay()
│   └── EnemySpawner : Node2D
│       - SpawnEnemy()
│
├── Projectile/
│   └── Projectile : Area2D
│       - CalculateDamage()
│       - OnBodyEntered()
│
└── Systems/
    ├── GameManager : Node
    │   - AddScore()
    │   - AddXP()
    │   - CheckLevelUp()
    │   - UnlockNewFunction()
    └── UI : CanvasLayer
        - UpdateDisplay()
```

### Collision Layers
- **Layer 1**: Player
- **Layer 2**: Enemy
- **Layer 3**: Projectile

### Scene Hierarchy
```
Main
├── GameManager
├── Player
│   └── Camera2D
├── EnemySpawner
└── UI
```

## Testing Checklist

### Manual Testing (Requires Godot Editor)
- [ ] Player moves with WASD keys
- [ ] Camera follows player
- [ ] Projectiles shoot towards cursor automatically
- [ ] Enemies spawn around player
- [ ] Enemies move towards player
- [ ] Enemies display their health number
- [ ] Projectiles damage enemies on collision
- [ ] Enemies die when health reaches 0
- [ ] Score increases on enemy kill
- [ ] XP increases on enemy kill
- [ ] Level up occurs at correct XP thresholds
- [ ] New functions unlock on level up
- [ ] Can switch functions with number keys 1-6
- [ ] UI displays correct information
- [ ] Different functions produce different damage patterns

### Expected Behavior
1. **Start**: Player in center, sin(x) function unlocked
2. **Movement**: Smooth 8-directional movement
3. **Combat**: Projectiles auto-shoot every 0.5 seconds
4. **Progression**: Kill ~10 enemies to reach level 2 (50 XP / 5 XP per kill)
5. **Function variety**: Each function creates different damage patterns based on distance

## Known Limitations
- No persistence (progress resets on restart)
- No visual effects for projectiles/impacts
- Simple geometric shapes for graphics
- No sound effects or music
- No pause menu
- No game over condition

## Future Enhancements (Out of Scope)
- Visual effects system
- Sound effects and music
- More enemy types
- Boss enemies
- Power-ups and upgrades
- Multiple weapon types
- Save/load system
- Main menu
- Settings menu
