using Godot;
using Godot.Collections;

public partial class BoxSpawnManager : Node
{
    [Export] private Array<Marker3D> _SpawnPoint = new Array<Marker3D>();
    [Export] private PackedScene book_box;

    private void SpawnBox()
    {
        if (_SpawnPoint.Count == 0 || book_box == null)
            return;

        // Kies een random spawn point
        int randomIndex = GD.RandRange(0, _SpawnPoint.Count - 1);
        Marker3D spawnPoint = _SpawnPoint[randomIndex];

        // Instantiate de box en zet de positie
        var box = book_box.Instantiate<Node3D>();
        box.GlobalPosition = spawnPoint.GlobalPosition;

        // Voeg toe aan de scene
        GetTree().Root.AddChild(box);

        GD.Print($"Spawned box at {spawnPoint.Name}");
    }
}