# PVS-hra

A Godot 4.5 top-down shooter with mathematical function-based projectiles and roguelike progression.

## 🎮 Features

- **Function-Based Combat**: Projectile trajectories and damage calculated using mathematical functions (sin, cos, linear, quadratic, logarithmic)
- **Roguelike Progression**: Level up by collecting XP, choose from random upgrades
- **Scaling Difficulty**: Enemies get faster and stronger as you score more points
- **Upgrade System**: Unlock new functions, increase stats, improve weapons
- **Score Tracking**: Compete for high scores with persistent end-game screen

## 🚀 Quick Start

### Prerequisites
- Godot 4.5+
- .NET SDK (for C# support)

### Setup
1. Open the project in Godot 4.5
2. Follow the setup guide in `SETUP_GUIDE.md` to add the new UI nodes
3. Build and run!

## 📚 Documentation

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)**: Complete integration instructions
- **[COMPLETION_SUMMARY.md](COMPLETION_SUMMARY.md)**: Feature overview and implementation details
- **[BALANCING_GUIDE.md](BALANCING_GUIDE.md)**: Difficulty tuning and balancing reference

## 🎯 Gameplay

1. **Survive**: Defeat enemies while dodging their attacks
2. **Collect XP**: Kill enemies to gain experience
3. **Level Up**: Choose from 3 random upgrades each level
4. **Unlock Functions**: Discover new projectile behaviors
5. **Score High**: Compete for the highest score before dying

## 🔧 Key Systems

### Score System
- Track points from enemy kills
- Drives difficulty scaling
- Displayed on end screen

### Upgrade System
Unlock on level up:
- More Health
- Faster Movement
- New Damage Functions
- New Trajectory Functions
- Weapon Improvements
- Backward Shooting

### Function Library
**Trajectories**: Constant, Sin, Cos, Linear, Quadratic, Logarithmic  
**Damage**: Constant, Sin, Cos, Linear, Quadratic, Logarithmic

All functions scale with upgrade levels for balanced progression.

### Difficulty Scaling
Enemies scale with score:
- Speed increases
- Damage increases
- Challenge grows organically

## 🎨 Project Structure

```
PVS-hra/
├── Assets/          # Art and sprites
├── Scenes/          # Godot scene files
├── Scripts/         # C# game logic
│   ├── Entities/    # Player, Enemy, Projectile
│   ├── UI/          # GameUI, UpgradeMenu, EndScreen, PauseMenu
│   ├── HealthSystem/
│   ├── MovementSystem/
│   ├── FunctionsManager/
│   ├── XPSystem/
│   └── ...
└── poznamky/        # Development notes

```

## 🎓 Development

This project was developed as part of a school project (PVS).

### Recent Additions (December 2025)
- ✅ Score system with UI
- ✅ Balancing (XP, score, difficulty scaling)
- ✅ Enemy spawn bounds (±4500 pixels)
- ✅ Complete upgrade menu system
- ✅ Expanded function library
- ✅ End screen with final score
- ✅ Function unlocking system
- ✅ Player stat upgrades

## 🐛 Troubleshooting

If you encounter issues:
1. Check the console for error messages
2. Verify all nodes are properly set up (see SETUP_GUIDE.md)
3. Ensure exported properties in inspector are assigned
4. Make sure .NET SDK is properly installed

## 📝 License

[Your license here]

## 👥 Contributors

[Your name/team here]
