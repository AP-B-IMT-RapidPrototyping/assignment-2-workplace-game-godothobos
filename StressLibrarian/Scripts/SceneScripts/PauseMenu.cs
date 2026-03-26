using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class PauseMenu : Control
{
    [Export] private PackedScene _mainMenu = GD.Load<PackedScene>("res://Scenes/Levels/main_menu.tscn");
    [Export] private AnimationPlayer _pauseAnimation;
    private bool _isPaused = false;

    public void TogglePauseMenu()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            GetTree().Paused = true;
            _pauseAnimation.Play("play");
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            GetTree().Paused = false;
            _pauseAnimation.PlayBackwards("play");
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }


    private void BackPressed()
    {
        TogglePauseMenu();
    }

    private void MenuPressed()
    {
        _isPaused = !_isPaused;
        GetTree().Paused = false;  
        GetTree().ChangeSceneToPacked(_mainMenu);
    }

    private void QuitPressed()
    {
        GetTree().Quit();
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("escape"))
        {
            TogglePauseMenu();
        }
    }

}
