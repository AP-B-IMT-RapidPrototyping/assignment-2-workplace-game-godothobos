using Godot;
using System;

public partial class TEMPDOOR : Area3D
{
    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("player"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile("res://scenes/levels/main_menu.tscn");
        }
    }
}
