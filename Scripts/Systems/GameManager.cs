using Godot;
using System;
using System.Collections.Generic;

namespace PVShra
{
    public partial class GameManager : Node
    {
        [Export] public int Score { get; private set; } = 0;
        [Export] public int XP { get; private set; } = 0;
        [Export] public int Level { get; private set; } = 1;
        
        private List<Functions.FunctionType> _unlockedFunctions = new List<Functions.FunctionType>();
        private int _currentFunctionIndex = 0;
        
        public Functions.FunctionType CurrentFunction => _unlockedFunctions[_currentFunctionIndex];
        
        // XP thresholds for leveling up
        private int[] _xpThresholds = { 50, 150, 300, 500, 750, 1000 };
        
        public override void _Ready()
        {
            // Start with Sin function unlocked
            _unlockedFunctions.Add(Functions.FunctionType.Sin);
        }
        
        public void AddScore(int points)
        {
            Score += points;
            UpdateUI();
        }
        
        public void AddXP(int xp)
        {
            XP += xp;
            CheckLevelUp();
            UpdateUI();
        }
        
        private void CheckLevelUp()
        {
            if (Level - 1 < _xpThresholds.Length && XP >= _xpThresholds[Level - 1])
            {
                Level++;
                UnlockNewFunction();
                GD.Print($"Level Up! Now level {Level}");
            }
        }
        
        private void UnlockNewFunction()
        {
            Functions.FunctionType[] allFunctions = {
                Functions.FunctionType.Sin,
                Functions.FunctionType.Cos,
                Functions.FunctionType.Linear,
                Functions.FunctionType.Quadratic,
                Functions.FunctionType.Tan,
                Functions.FunctionType.Exponential
            };
            
            foreach (var func in allFunctions)
            {
                if (!_unlockedFunctions.Contains(func))
                {
                    _unlockedFunctions.Add(func);
                    GD.Print($"Unlocked new function: {Functions.FunctionFactory.CreateFunction(func).GetName()}");
                    break;
                }
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            // Switch between unlocked functions with number keys
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
                {
                    int index = (int)(keyEvent.Keycode - Key.Key1);
                    if (index < _unlockedFunctions.Count)
                    {
                        _currentFunctionIndex = index;
                        GD.Print($"Switched to function: {Functions.FunctionFactory.CreateFunction(CurrentFunction).GetName()}");
                        UpdateUI();
                    }
                }
            }
        }
        
        private void UpdateUI()
        {
            var ui = GetNodeOrNull<UI>("/root/Main/UI");
            if (ui != null)
            {
                ui.UpdateDisplay(Score, XP, Level, CurrentFunction, _unlockedFunctions);
            }
        }
    }
}
