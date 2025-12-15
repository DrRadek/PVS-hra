# Integration Checklist

Use this checklist to ensure all systems are properly integrated into your Godot scenes.

## ☐ Step 1: Add Core Managers to Main Scene

Open your main game scene (e.g., `main.tscn` or `main_main.tscn`)

### ScoreManager
- [ ] Add a new **Node** as a child of the root
- [ ] Rename it to exactly: `ScoreManager`
- [ ] In Inspector, attach script: `Scripts/ScoreManager.cs`
- [ ] Verify the script is attached (should show in Inspector)

### UpgradeMenu
- [ ] Add a new **Control** node as a child of the root (or under your UI layer)
- [ ] Rename it to exactly: `UpgradeMenu`
- [ ] In Inspector, attach script: `Scripts/UI/UpgradeMenu.cs`
- [ ] In Inspector, set Layout → Preset to **Full Rect**
- [ ] Set Visible to `false` (will be shown automatically on level up)

### EndScreen
- [ ] Add a new **Control** node as a child of the root (or under your UI layer)
- [ ] Rename it to exactly: `EndScreen`
- [ ] In Inspector, attach script: `Scripts/UI/EndScreen.cs`
- [ ] In Inspector, set Layout → Preset to **Full Rect**
- [ ] Set Visible to `false` (will be shown automatically on death)

## ☐ Step 2: Update GameUI

Find your GameUI node in the scene:

### Add Score Label
- [ ] Add a new **Label** node as a child of GameUI
- [ ] Rename it to: `ScoreLabel`
- [ ] Position it in the top-right corner (or wherever you prefer)
- [ ] Set text to `"Score: 0"` as a placeholder
- [ ] Adjust font size if needed (recommend 16-24)

### Link Score Label
- [ ] Select the GameUI node
- [ ] In Inspector, find the exported property `scoreLabel`
- [ ] Drag the ScoreLabel node into this property (or click and select it)
- [ ] Verify the property shows the correct node path

## ☐ Step 3: Configure Player Scene

Open your player scene (e.g., `player.tscn`):

### Find/Create Required Nodes
- [ ] Locate your **HealthManager** node
- [ ] Locate your **FunctionsManager** node
- [ ] Locate your **LevelManager** node
- [ ] Locate your **MovableObject** node (might be under Scripts or Movement)

### Set Player Export Properties
Select the Player (root) node:
- [ ] In Inspector, verify `healthManagerLocation` points to your HealthManager
- [ ] Verify `functionsManagerLocation` points to your FunctionsManager
- [ ] Verify `levelManager` is assigned to your LevelManager node
- [ ] Set `movableObjectLocation` to point to your MovableObject node
- [ ] If you have a storage node, verify `storageNode` is set

### Configure Health
Select the Health node (usually under HealthManager):
- [ ] In Inspector, set `maxHealth` to `100` (or your preferred starting HP)
- [ ] Set `health` to `100` (should match maxHealth)
- [ ] Verify `updateGlobalGameUI` is checked **true** for the player

## ☐ Step 4: Configure Enemy Scene

Open your enemy scene (e.g., `enemy.tscn`):

### Set Enemy Export Properties
Select the Enemy (root) node:
- [ ] In Inspector, set `baseScoreReward` to `50` (adjust for balance)
- [ ] Set `baseExpDropAmount` to `10` (adjust for balance)
- [ ] Verify other properties are correctly set

## ☐ Step 5: Configure GameManager

In your main scene, find the GameManager node:

### Set Map Bounds
- [ ] In Inspector, set `MapBounds` to `4500` (or adjust as needed)
- [ ] This prevents enemies from spawning outside ±4500 pixels from origin

## ☐ Step 6: Build and Test

### Build the Project
- [ ] In Godot, click **Build** (or Ctrl+Shift+B)
- [ ] Wait for compilation to complete
- [ ] Check for any errors in the output panel
- [ ] If errors appear, verify all script paths are correct

### Run Initial Test
- [ ] Press **F5** to run the game
- [ ] Check the console for any red errors
- [ ] Verify the game starts without crashes

## ☐ Step 7: Test Each System

### Score System
- [ ] Score UI appears at game start showing "Score: 0"
- [ ] Kill an enemy
- [ ] Verify score increases (should show 50 by default)
- [ ] Kill multiple enemies, score continues to increase

### XP and Level System
- [ ] Kill enemies to collect XP orbs
- [ ] Watch the XP bar fill up
- [ ] When you level up, the upgrade menu should appear
- [ ] Game should pause when menu is shown

### Upgrade Menu
- [ ] Upgrade menu shows 3 different options
- [ ] Each option has a name and description
- [ ] Click one option
- [ ] Menu closes and game unpauses
- [ ] The upgrade effect applies (check console for confirmation)

### Difficulty Scaling
- [ ] Play for a while, building up score
- [ ] Notice enemies getting faster over time
- [ ] Higher score = more aggressive enemies

### End Screen
- [ ] Let your player die (let enemies hit you)
- [ ] End screen appears showing "GAME OVER"
- [ ] Final score is displayed correctly
- [ ] "NEW GAME" button is visible
- [ ] Click "NEW GAME"
- [ ] Game restarts with score reset to 0
- [ ] Player respawns at starting position

### Function Unlocking
- [ ] Level up and choose "Unlock Trajectory" or "Unlock Damage"
- [ ] Verify you can access the new function in pause menu
- [ ] Test that the function works when assigned to an attack

## ☐ Step 8: Verify All Features

### Core Features Checklist
- [ ] Score displays and updates
- [ ] XP collection works
- [ ] Level up triggers upgrade menu
- [ ] All 6+ upgrade types available
- [ ] Health upgrades increase max HP
- [ ] Speed upgrades make player faster
- [ ] Damage upgrades improve attack power
- [ ] Function unlocks add to available pool
- [ ] Backward shooting can be unlocked
- [ ] Enemy difficulty scales with score
- [ ] Enemies spawn within map bounds
- [ ] Death triggers end screen
- [ ] Restart button works correctly

## 🐛 Common Issues and Solutions

### "UpgradeMenu not found" in console
**Fix**: Ensure the UpgradeMenu node is named exactly "UpgradeMenu" and exists in the scene

### Upgrade menu doesn't appear on level up
**Fix**: 
1. Check console for errors
2. Verify UpgradeMenu.cs script is attached
3. Ensure FunctionsManager exists in player scene

### Score doesn't display
**Fix**:
1. Check GameUI's `scoreLabel` property is set
2. Verify ScoreLabel node exists
3. Ensure ScoreManager node exists in main scene

### Health upgrades don't work
**Fix**:
1. Verify Player has `movableObjectLocation` set
2. Check that Health node is a child of HealthManager
3. Look for errors in console when upgrade is selected

### End screen doesn't show on death
**Fix**:
1. Ensure EndScreen node exists and has script attached
2. Check that it's set to Visible = false initially
3. Verify Player's OnDeath() method is being called (check console)

### Enemies spawn outside map
**Fix**:
1. Check GameManager's `MapBounds` is set correctly
2. Verify the updated GameManager.cs code is being used

### Game crashes on level up
**Fix**:
1. Check console for specific error
2. Verify all Player export properties are set
3. Ensure FunctionsManager and LevelManager exist

## ✅ Final Verification

Once all checkboxes are complete:
- [ ] Play for 5+ minutes
- [ ] Reach at least level 3
- [ ] Try different upgrade combinations
- [ ] Test death and restart
- [ ] Verify no errors in console
- [ ] Confirm all UI elements display correctly

## 🎉 Success!

If all checks pass, your game is fully integrated and ready to play!

See `BALANCING_GUIDE.md` for tips on adjusting difficulty and progression.
