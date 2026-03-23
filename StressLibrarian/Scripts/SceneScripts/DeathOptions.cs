using Godot;
using System;

public partial class DeathOptions : Control
{
    [Export] private PackedScene _mainMenu;
    [Export] private PackedScene _retry;
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }


    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToPacked(_mainMenu);
    }
    private void OnRetryPressed()
    {
        GameManager.ResetGame();
        GetTree().ChangeSceneToPacked(_retry);
    }
}
