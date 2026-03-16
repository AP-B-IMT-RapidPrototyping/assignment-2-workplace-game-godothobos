using Godot;
using System;

public partial class InteractBookShelf : Node3D
{
    [Export] private Area3D Area3D;
    public BookGenre shelfGenre;


    public override void _Ready()
    {
        GD.Print("SHELF");
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print("ok");
    }

    private void CorrectBook(BookBox book)
    {
        GD.Print("good boy");
    }

    private void WrongBook(BookBox book)
    {
        GD.Print("wrong book");
    }

}
