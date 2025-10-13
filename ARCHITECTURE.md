# Game Flow Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Main Scene                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ GameManager  │  │    Player    │  │EnemySpawner  │      │
│  │              │  │              │  │              │      │
│  │ - Score      │  │ - Movement   │  │ - Spawning   │      │
│  │ - XP         │◄─┤ - Shooting   │  │   Timer      │      │
│  │ - Level      │  │ - Auto Aim   │  └──────┬───────┘      │
│  │ - Functions  │  └──────┬───────┘         │              │
│  └──────┬───────┘         │                 │              │
│         │                 │                 │              │
│         │                 │    Spawns       │              │
│         │                 │        ▼        │              │
│         │                 │   ┌─────────┐   │              │
│         │                 │   │  Enemy  │   │              │
│         │                 │   │         │◄──┘              │
│         │                 │   │ - HP    │                  │
│         │                 │   │ - Move  │                  │
│         │                 │   └────▲────┘                  │
│         │                 │        │                       │
│         │                 │  Shoots│  Collides             │
│         │                 ▼        │                       │
│         │           ┌──────────┐   │                       │
│         │           │Projectile│───┘                       │
│         │           │          │                           │
│         │           │ - f(x)   │                           │
│         │           │ - Damage │                           │
│         │           └──────────┘                           │
│         │                                                   │
│         │  Updates                                          │
│         ▼                                                   │
│    ┌────────┐                                              │
│    │   UI   │                                              │
│    │        │                                              │
│    │ Display│                                              │
│    └────────┘                                              │
└─────────────────────────────────────────────────────────────┘
```

## Game Loop Flow

```
┌──────────────────────────────────────────────────────────────┐
│                     Game Start                                │
└───────────────────────────┬──────────────────────────────────┘
                            │
                            ▼
        ┌───────────────────────────────────────┐
        │ Initialize:                            │
        │ - Player at center (640, 360)         │
        │ - Unlock sin(x) function              │
        │ - Start spawn timer                   │
        │ - Show initial UI                     │
        └───────────────┬───────────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────────┐
        │         Game Loop (60 FPS)            │
        │  ┌─────────────────────────────────┐  │
        │  │ Player Input                    │  │
        │  │  - Read WASD for movement       │  │
        │  │  - Read Mouse for aim           │  │
        │  │  - Read 1-6 for function switch │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ Player Update                   │  │
        │  │  - Apply movement velocity      │  │
        │  │  - Decrease attack timer        │  │
        │  │  - Shoot if timer <= 0          │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ Enemy Spawner Update            │  │
        │  │  - Increase spawn timer         │  │
        │  │  - Spawn enemy if timer >= 2.0  │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ Enemy Update                    │  │
        │  │  - Calculate direction to player│  │
        │  │  - Move towards player          │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ Projectile Update               │  │
        │  │  - Move in direction            │  │
        │  │  - Track distance               │  │
        │  │  - Check for collision          │  │
        │  │  - Apply damage if hit          │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ Game State Update               │  │
        │  │  - Check enemy deaths           │  │
        │  │  - Add score and XP             │  │
        │  │  - Check level up               │  │
        │  │  - Unlock functions             │  │
        │  └──────────────┬──────────────────┘  │
        │                 ▼                      │
        │  ┌─────────────────────────────────┐  │
        │  │ UI Update                       │  │
        │  │  - Display score/XP/level       │  │
        │  │  - Show current function        │  │
        │  │  - List unlocked functions      │  │
        │  └─────────────────────────────────┘  │
        └───────────────────────────────────────┘
                        │
                        └──────┐
                               │
                               ▼
                        (Loop Continues)
```

## Damage Calculation Flow

```
┌───────────────────────────────────────────────────────────┐
│                  Projectile Hits Enemy                     │
└──────────────────────────┬────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────┐
        │  Get Distance Traveled            │
        │  distance = current - start       │
        └──────────┬───────────────────────┘
                   │
                   ▼
        ┌──────────────────────────────────┐
        │  Normalize Distance               │
        │  x = distance / 100               │
        └──────────┬───────────────────────┘
                   │
                   ▼
        ┌──────────────────────────────────┐
        │  Evaluate Function                │
        │  value = function.Evaluate(x)     │
        │                                   │
        │  Examples:                        │
        │  - sin(x)  → -1.0 to 1.0         │
        │  - x       → proportional         │
        │  - x²      → rapid growth         │
        │  - e^(x/2) → exponential          │
        └──────────┬───────────────────────┘
                   │
                   ▼
        ┌──────────────────────────────────┐
        │  Calculate Damage                 │
        │  damage = max(1, |value| * 10)   │
        │                                   │
        │  Ensures:                         │
        │  - Minimum damage = 1             │
        │  - Absolute value (no negative)   │
        │  - Scaled by 10 for balance       │
        └──────────┬───────────────────────┘
                   │
                   ▼
        ┌──────────────────────────────────┐
        │  Apply to Enemy                   │
        │  enemy.Health -= damage           │
        └──────────┬───────────────────────┘
                   │
                   ▼
        ┌──────────────────────────────────┐
        │  Check Death                      │
        │  if Health <= 0:                  │
        │    - Add Score (+10)              │
        │    - Add XP (+5)                  │
        │    - Destroy Enemy                │
        └───────────────────────────────────┘
```

## Function Unlock Progression

```
Start
  │
  ▼
Level 1 (0 XP)
  │ Unlocked: sin(x)
  │
  ▼ Kill ~10 enemies (50 XP needed)
  │
Level 2 (50 XP)
  │ Unlock: cos(x)
  │
  ▼ Kill ~20 more enemies (100 more XP)
  │
Level 3 (150 XP)
  │ Unlock: x (linear)
  │
  ▼ Kill ~30 more enemies (150 more XP)
  │
Level 4 (300 XP)
  │ Unlock: x² (quadratic)
  │
  ▼ Kill ~40 more enemies (200 more XP)
  │
Level 5 (500 XP)
  │ Unlock: tan(x)
  │
  ▼ Kill ~50 more enemies (250 more XP)
  │
Level 6 (750 XP)
  │ Unlock: e^(x/2) (exponential)
  │
  ▼ Kill ~50 more enemies (250 more XP)
  │
Level 7 (1000 XP)
  │ All functions unlocked!
  │
  ▼
Continue playing...
```

## Collision Layers

```
Layer 1: Player
  ├─ Collides with: Enemy (Layer 2)
  └─ Ignores: Projectile (Layer 3)

Layer 2: Enemy
  ├─ Collides with: Player (Layer 1), Projectile (Layer 3)
  └─ Ignores: Other Enemies

Layer 3: Projectile
  ├─ Collides with: Enemy (Layer 2)
  └─ Ignores: Player (Layer 1), Other Projectiles
```
