using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class BookBox : RigidBody3D
{
    [Export] private MeshInstance3D _textMesh;
    public BookGenre bookGenre;


    public override void _Ready()
    {
        var values = Enum.GetValues(typeof(BookGenre));
        bookGenre = (BookGenre)values.GetValue(GD.RandRange(0, values.Length - 1));
        GD.Print($"book: {bookGenre}");

        if (_textMesh.Mesh != null)
        {
            _textMesh.Mesh = _textMesh.Mesh.Duplicate() as Mesh;
        }

        if (_textMesh.Mesh is TextMesh textMesh)
        {
            textMesh.Text = bookGenre.ToString();
        }
    }
}
