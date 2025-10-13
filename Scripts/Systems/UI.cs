using Godot;
using System;
using System.Collections.Generic;

namespace PVShra
{
    public partial class UI : CanvasLayer
    {
        private Label _scoreLabel;
        private Label _xpLabel;
        private Label _levelLabel;
        private Label _functionLabel;
        private Label _functionsListLabel;
        
        public override void _Ready()
        {
            // Create UI labels
            _scoreLabel = new Label();
            _scoreLabel.Position = new Vector2(10, 10);
            _scoreLabel.AddThemeFontSizeOverride("font_size", 20);
            AddChild(_scoreLabel);
            
            _xpLabel = new Label();
            _xpLabel.Position = new Vector2(10, 40);
            _xpLabel.AddThemeFontSizeOverride("font_size", 20);
            AddChild(_xpLabel);
            
            _levelLabel = new Label();
            _levelLabel.Position = new Vector2(10, 70);
            _levelLabel.AddThemeFontSizeOverride("font_size", 20);
            AddChild(_levelLabel);
            
            _functionLabel = new Label();
            _functionLabel.Position = new Vector2(10, 100);
            _functionLabel.AddThemeFontSizeOverride("font_size", 20);
            _functionLabel.AddThemeColorOverride("font_color", Colors.Yellow);
            AddChild(_functionLabel);
            
            _functionsListLabel = new Label();
            _functionsListLabel.Position = new Vector2(10, 130);
            _functionsListLabel.AddThemeFontSizeOverride("font_size", 16);
            AddChild(_functionsListLabel);
        }
        
        public void UpdateDisplay(int score, int xp, int level, Functions.FunctionType currentFunction, List<Functions.FunctionType> unlockedFunctions)
        {
            _scoreLabel.Text = $"Score: {score}";
            _xpLabel.Text = $"XP: {xp}";
            _levelLabel.Text = $"Level: {level}";
            
            var currentFunc = Functions.FunctionFactory.CreateFunction(currentFunction);
            _functionLabel.Text = $"Current Function: {currentFunc.GetName()}";
            
            // Show all unlocked functions
            string functionsList = "Unlocked Functions:\n";
            for (int i = 0; i < unlockedFunctions.Count; i++)
            {
                var func = Functions.FunctionFactory.CreateFunction(unlockedFunctions[i]);
                functionsList += $"  [{i + 1}] {func.GetName()}\n";
            }
            _functionsListLabel.Text = functionsList;
        }
    }
}
