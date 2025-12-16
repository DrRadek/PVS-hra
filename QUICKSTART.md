# 🚀 Quick Start - Getting Your Game Running

## TL;DR - Absolute Minimum to Get Started

1. **Add 3 nodes to your main scene**:
   - Node named `ScoreManager` with `ScoreManager.cs` script
   - Control named `UpgradeMenu` with `UpgradeMenu.cs` script (Full Rect, Visible=false)
   - Control named `EndScreen` with `EndScreen.cs` script (Full Rect, Visible=false)

2. **Add to GameUI**:
   - Label named `ScoreLabel`
   - Set GameUI's `scoreLabel` export to point to it

3. **Set Player exports**:
   - Set `movableObjectLocation` to your MovableObject node path

4. **Build & Run!**

---

## 5-Minute Setup

### Step 1: Open Main Scene (1 min)
Open `main.tscn` or `main_main.tscn` in Godot editor.

### Step 2: Add Managers (2 min)
Right-click on the root node → Add Child Node:

1. **Add ScoreManager**:
   - Type: `Node`
   - Name: `ScoreManager`
   - Script: Drag `Scripts/ScoreManager.cs` onto it

2. **Add UpgradeMenu**:
   - Type: `Control`
   - Name: `UpgradeMenu`
   - Script: Drag `Scripts/UI/UpgradeMenu.cs` onto it
   - In Inspector: Layout → Full Rect
   - Set Visible to OFF (unchecked)

3. **Add EndScreen**:
   - Type: `Control`
   - Name: `EndScreen`
   - Script: Drag `Scripts/UI/EndScreen.cs` onto it
   - In Inspector: Layout → Full Rect
   - Set Visible to OFF (unchecked)

### Step 3: Update GameUI (1 min)
Find your GameUI node:
- Add child: `Label` named `ScoreLabel`
- Position it in top-right corner
- Select GameUI node
- In Inspector, set `scoreLabel` → drag ScoreLabel here

### Step 4: Update Player (1 min)
Open `player.tscn`:
- Select Player node
- Find the `MovableObject` node in your scene (usually under Scripts/)
- In Player Inspector, set `movableObjectLocation` to this node's path

### Step 5: Run!
- Press F6 to build
- Press F5 to run
- Play and test!

---

## What You Get

### Immediately Working:
✅ Score display in UI  
✅ Score increases when killing enemies  
✅ XP collection  
✅ Level up trigger  
✅ Upgrade menu with 3 choices  
✅ All upgrade types functional  
✅ Enemy difficulty scaling  
✅ Death → End screen  
✅ Restart functionality  

### Unlockable Functions:
- Start: Constant damage & trajectory
- Unlock: Sin, Cos, Linear, Quadratic, Log
- Upgrade: Health, Speed, Damage, Fire Rate

---

## Testing Your Setup

### Test 1: Score (30 seconds)
1. Start game
2. Kill 1 enemy
3. ✅ Score should show "Score: 50"

### Test 2: Level Up (2 minutes)
1. Start game
2. Kill 10 enemies (collect XP)
3. ✅ Upgrade menu appears
4. Click any option
5. ✅ Menu closes, game continues

### Test 3: Death (1 minute)
1. Start game
2. Let enemies kill you
3. ✅ End screen appears with score
4. Click "NEW GAME"
5. ✅ Game restarts, score resets

If all 3 tests pass → **You're done! 🎉**

---

## Troubleshooting

### "Upgrade menu doesn't appear"
- Check: Is UpgradeMenu node named exactly "UpgradeMenu"?
- Check: Does it have the script attached?
- Check: Console for errors?

### "Score doesn't show"
- Check: Did you add ScoreLabel to GameUI?
- Check: Did you set the scoreLabel export property?
- Check: Is ScoreManager node in the scene?

### "Player upgrades don't work"
- Check: Did you set movableObjectLocation in Player?
- Check: Does Player have MovableObject node?
- Check: Console for error messages?

---

## Next Steps

Once basic setup works:

1. **Balance the game**:
   - See `BALANCING_GUIDE.md`
   - Adjust enemy rewards
   - Tune difficulty scaling

2. **Customize visuals**:
   - Position UI elements
   - Adjust fonts and colors
   - Add your art

3. **Playtest**:
   - Try different upgrade paths
   - Find optimal difficulty
   - Discover fun combinations

---

## Documentation Index

- **This file**: Quick setup
- `INTEGRATION_CHECKLIST.md`: Detailed checklist
- `SCENE_STRUCTURE.md`: Visual scene hierarchy
- `SETUP_GUIDE.md`: Complete integration guide
- `COMPLETION_SUMMARY.md`: Feature overview
- `BALANCING_GUIDE.md`: Difficulty tuning
- `README.md`: Project overview

---

## Need Help?

1. Check console for error messages
2. Verify node names match exactly
3. Ensure scripts are attached
4. Review INTEGRATION_CHECKLIST.md
5. Check export properties are set

---

**Estimated setup time**: 5-10 minutes  
**Difficulty**: Easy  
**Prerequisites**: Basic Godot knowledge

Good luck and have fun! 🎮
