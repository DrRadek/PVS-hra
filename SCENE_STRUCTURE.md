# Scene Structure Guide

This document shows exactly how your scenes should be structured after integration.

## Main Game Scene Structure

```
Main (Node2D or Node)
├── GameManager (Node2D) [GameManager.cs]
├── ScoreManager (Node) [ScoreManager.cs] ← ADD THIS
├── BackgroundMover (?)
├── Player (RigidBody2D)
│   └── (see Player Scene Structure below)
├── GameUI (Node or Control)
│   ├── LevelLabel (Label)
│   ├── LevelProgressBar (TextureProgressBar)
│   ├── HpLabel (Label)
│   ├── HpProgressBar (TextureProgressBar)
│   ├── ScoreLabel (Label) ← ADD THIS
│   └── PauseMenu (Control) [PauseMenu.cs]
├── UpgradeMenu (Control) [UpgradeMenu.cs] ← ADD THIS (Full Rect, Visible=false)
└── EndScreen (Control) [EndScreen.cs] ← ADD THIS (Full Rect, Visible=false)
```

## Player Scene Structure

```
Player (RigidBody2D) [Player.cs]
├── Sprite (Sprite2D)
├── CollisionShape (CollisionShape2D)
├── Scripts (Node)
│   ├── HealthManager (Node) [HealthManager.cs]
│   │   └── Health (Node) [Health.cs]
│   │       └── (maxHealth: 100, health: 100, updateGlobalGameUI: true)
│   ├── FunctionsManager (Node) [FunctionsManager.cs]
│   │   └── RotationNode (Node2D)
│   │       └── (projectiles spawn here)
│   ├── LevelManager (Node) [LevelManager.cs]
│   ├── MovableObject (Node) [MovableObject.cs] ← LOCATE THIS
│   │   └── (speed: 200)
│   └── CollectibleCollector (Node) [CollectibleCollector.cs]
└── ...other components

Player Export Properties (set in Inspector):
├── healthManagerLocation → "Scripts/HealthManager"
├── functionsManagerLocation → "Scripts/FunctionsManager"  
├── movableObjectLocation → "Scripts/MovableObject" ← SET THIS
├── levelManager → (drag LevelManager node here)
└── storageNode → (usually set at runtime)
```

## Enemy Scene Structure

```
Enemy (RigidBody2D) [Enemy.cs]
├── Sprite (Sprite2D)
├── CollisionShape (CollisionShape2D)
├── Scripts (Node)
│   ├── HealthManager (Node) [HealthManager.cs]
│   │   └── Health (Node) [Health.cs]
│   │       └── (updateGlobalGameUI: false)
│   ├── TargetFollower (Node) [TargetFollower.cs]
│   └── MovableObject (Node) [MovableObject.cs]
│       └── (speed: 100-150)
└── ...other components

Enemy Export Properties (set in Inspector):
├── healthManagerLocation → "Scripts/HealthManager"
├── targetFollower → (drag TargetFollower node here)
├── exp → (PackedScene: Exp.tscn)
├── baseExpDropAmount → 10 ← CONFIGURE THIS
├── baseScoreReward → 50 ← CONFIGURE THIS
└── target → (set at runtime by GameManager)
```

## GameUI Scene Structure

```
GameUI (Node or Control)
├── LevelLabel (Label)
│   └── Text: "level 1 (100 xp left)"
├── LevelProgressBar (TextureProgressBar)
│   └── (Value: 0, MaxValue: 100)
├── HpLabel (Label)
│   └── Text: "hp 100/100"
├── HpProgressBar (TextureProgressBar)
│   └── (Value: 100, MaxValue: 100)
├── ScoreLabel (Label) ← ADD THIS
│   ├── Text: "Score: 0"
│   ├── Position: Top-right corner
│   └── Font Size: 20-24
└── PauseMenu (Control)
    └── ...existing pause menu structure

GameUI Export Properties (set in Inspector):
├── levelLabel → (drag LevelLabel here)
├── levelProgressBar → (drag LevelProgressBar here)
├── hpLabel → (drag HpLabel here)
├── hpProgressBar → (drag HpProgressBar here)
└── scoreLabel → (drag ScoreLabel here) ← SET THIS
```

## Visual Layout Examples

### GameUI Layout (Suggested)
```
┌──────────────────────────────────────────────┐
│ LEVEL 3 (50 xp left)    [████████░░] Score: 1250 │
│ HP 75/100               [████████░░░░░]         │
│                                                │
│                                                │
│          [Game Play Area]                      │
│                                                │
│                                                │
└──────────────────────────────────────────────┘
```

### UpgradeMenu Layout (Auto-generated)
```
┌──────────────────────────────────────────────┐
│                                                │
│        ┌────────────────────────────┐          │
│        │                            │          │
│        │   LEVEL UP! Choose Upgrade │          │
│        │   ─────────────────────────│          │
│        │                            │          │
│        │  ┌──────────────────────┐ │          │
│        │  │ More Health          │ │          │
│        │  │ +25 Max HP...        │ │          │
│        │  └──────────────────────┘ │          │
│        │  ┌──────────────────────┐ │          │
│        │  │ Faster Movement      │ │          │
│        │  │ +20% speed           │ │          │
│        │  └──────────────────────┘ │          │
│        │  ┌──────────────────────┐ │          │
│        │  │ Unlock: sin(x)       │ │          │
│        │  │ New trajectory       │ │          │
│        │  └──────────────────────┘ │          │
│        │                            │          │
│        └────────────────────────────┘          │
│                                                │
└──────────────────────────────────────────────┘
```

### EndScreen Layout (Auto-generated)
```
┌──────────────────────────────────────────────┐
│                                                │
│                                                │
│             GAME OVER                          │
│                                                │
│          Final Score: 5420                     │
│                                                │
│          ┌──────────────┐                      │
│          │  NEW GAME    │                      │
│          └──────────────┘                      │
│                                                │
│                                                │
└──────────────────────────────────────────────┘
```

## Quick Setup Paths

### If you can't find the nodes:

**MovableObject**: Usually at:
- `Player/Scripts/MovableObject`
- `Player/Movement/MovableObject`
- Search for nodes with script `MovableObject.cs`

**HealthManager**: Usually at:
- `Player/Scripts/HealthManager`
- `Player/Health/HealthManager`
- Search for nodes with script `HealthManager.cs`

**FunctionsManager**: Usually at:
- `Player/Scripts/FunctionsManager`
- `Player/Functions/FunctionsManager`
- Search for nodes with script `FunctionsManager.cs`

### How to find a node in your scene:
1. Open the scene in Godot
2. Press `Ctrl+F` to search
3. Type the node name or script name
4. Select the found node
5. Note its path in the scene tree
6. Use that path in the export property

## Node Naming Conventions

For the systems to work correctly, these nodes MUST be named exactly:
- `ScoreManager` (for ScoreManager.Instance)
- `UpgradeMenu` (for UpgradeMenu.Instance)
- `EndScreen` (for EndScreen.Instance)
- `GameUI` (for GameUI.Instance)
- `GameManager` (for GameManager.Instance)

## Export Property Paths

When setting NodePath exports, use one of these formats:
- Relative path: `"Scripts/HealthManager"`
- Relative with dots: `"../SomeNode"`
- Absolute (less common): `"/root/Main/Player/Scripts/HealthManager"`

Most paths in this project use the relative format.

## Verification

After setting up your scenes, verify:
- [ ] All new nodes are created
- [ ] All scripts are attached
- [ ] All export properties are set
- [ ] UI elements are positioned visibly
- [ ] Control nodes have correct Layout settings
- [ ] Visible flags are set correctly

Run the game and check the console for:
- No red error messages
- System ready messages
- All Instance assignments succeed

## Tips

1. **Save often**: Save your scene after each major change
2. **Test incrementally**: Add one system at a time and test
3. **Check Inspector**: Verify all exported properties show correct values
4. **Console is your friend**: Watch for errors and debug prints
5. **Use scene search**: Ctrl+F to find nodes quickly in complex scenes
