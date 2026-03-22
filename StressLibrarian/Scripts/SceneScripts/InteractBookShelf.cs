using Godot;
using System;

public partial class InteractBookShelf : Node3D
{
    [Export] private MeshInstance3D _textMesh;
    [Export] private Node3D _booksFull;
    [Export] public BookGenre shelfGenre;
    [Export] public bool Randomize = true;
    [Export] private AudioStreamPlayer3D fillSound;

    private float _reduceStress = 10f;


    public override void _Ready()
    {
        AddToGroup("bookshelves");

        var values = Enum.GetValues(typeof(BookGenre));

        int shelfCount = GetTree().GetNodesInGroup("bookshelves").Count;
        int genreCount = values.Length;

        if (Randomize)
        {
            if (shelfCount <= genreCount)
            {
                bool unique = false;

                while (!unique)
                {
                    shelfGenre = (BookGenre)values.GetValue(GD.RandRange(0, genreCount - 1));
                    unique = true;

                    foreach (Node node in GetTree().GetNodesInGroup("bookshelves"))
                    {
                        if (node == this)
                            continue;

                        if (node is InteractBookShelf shelf)
                        {
                            if (shelf.shelfGenre == shelfGenre)
                            {
                                unique = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                shelfGenre = (BookGenre)values.GetValue(GD.RandRange(0, genreCount - 1));
            }
        }

        /* GD.Print($"shelf: {shelfGenre}"); */

        if (_textMesh.Mesh != null)
        {
            _textMesh.Mesh = _textMesh.Mesh.Duplicate() as Mesh;
        }

        if (_textMesh.Mesh is TextMesh textMesh)
        {
            textMesh.Text = shelfGenre.ToString();
        }
    }

    private void OnBodyEnteredPlayer(Node3D body)
    {
        if (!body.IsInGroup("player"))
            return;


        foreach (Node node in GetTree().GetNodesInGroup("interact_npc"))
        {
            if (node is Npc npc)
            {
                npc.OnPlayerReachedCorrectShelf(shelfGenre);
            }
        }
    }

    private void OnBodyEnteredBooks(Node3D body)
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
    }

    private void CorrectBook(BookBox book)
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null)
            player.HandleForceDropObject(book);

        book.QueueFree();
        fillSound.Play();
        GameManager.Stress -= _reduceStress;
        GameManager._highScore++;
        FillBooks();
    }

    private void WrongBook(BookBox book)
    {

    }


    private void EmptyBooks()
    {
        _booksFull.Visible = false;
    }

    private void FillBooks()
    {
        _booksFull.Visible = true;
    }
}
