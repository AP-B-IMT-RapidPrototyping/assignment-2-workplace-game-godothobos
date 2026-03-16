using Godot;
using System;

public partial class InteractBookShelf : Node3D
{
    [Export] private MeshInstance3D _textMesh;
    [Export] private Area3D Area3D;
    public BookGenre shelfGenre;


    public override void _Ready()
    {
        var values = Enum.GetValues(typeof(BookGenre));
        shelfGenre = (BookGenre)values.GetValue(GD.RandRange(0, values.Length - 1));
        GD.Print($"shelf: {shelfGenre}");

        if (_textMesh.Mesh != null)
        {
            _textMesh.Mesh = _textMesh.Mesh.Duplicate() as Mesh;
        }

        if (_textMesh.Mesh is TextMesh textMesh)
        {
            textMesh.Text = shelfGenre.ToString();
        }
    }

    private void OnBodyEntered(RigidBody3D body)
    {
        if (body.IsInGroup("interact_pickup"))
        {
            if (body is BookBox book)
            {
                if (book.bookGenre == shelfGenre)
                    CorrectBook(book);
                else
                {
                    WrongBook(book);
                }
            }
        }
        GD.Print("ok");
    }

    private void CorrectBook(BookBox book)
    {
        GD.Print("good boy");
        
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
        player.HandleForceDropObject(book);
        
        book.QueueFree();
    }

    private void WrongBook(BookBox book)
    {
        GD.Print("wrong book");
    }

}
