using Godot;
using Godot.Collections;

public partial class BoxSpawn : Node
{
    [Export] private Array<Marker3D> _SpawnPoint = new Array<Marker3D>();
    [Export] private PackedScene book_box;

    [Export] public float SpawnDelayMin = 2f;
    [Export] public float SpawnDelayMax = 5f;

    private Dictionary<Marker3D, Node3D> _spawnedBoxes = new Dictionary<Marker3D, Node3D>();
    private Dictionary<Marker3D, Timer> _timers = new Dictionary<Marker3D, Timer>();

    public override void _Ready()
    {
        foreach (var marker in _SpawnPoint)
        {
            // Timer per marker
            Timer t = new Timer();
            t.OneShot = true;
            AddChild(t);

            // Timer roept GEEN SpawnBox() aan
            // wel wrapper zonder parameters
            t.Timeout += () => OnTimerTimeout(marker);

            _timers[marker] = t;

            StartSpawnTimer(marker);
        }
    }

    private void OnTimerTimeout(Marker3D marker)
    {
        SpawnBox(marker);
    }

    private void StartSpawnTimer(Marker3D marker)
    {
        float delay = (float)GD.RandRange(SpawnDelayMin, SpawnDelayMax);
        _timers[marker].Start(delay);
        GD.Print($"Timer for {marker.Name} started with delay: {delay}");
    }

    private void SpawnBox(Marker3D spawnPoint)
    {
        // Check of er al een box is voor deze marker
        if (_spawnedBoxes.ContainsKey(spawnPoint))
        {
            var box = _spawnedBoxes[spawnPoint];

            if (box != null && IsInstanceValid(box))
                return;

            _spawnedBoxes.Remove(spawnPoint);
        }

        if (book_box == null)
            return;

        var boxInstance = book_box.Instantiate<Node3D>();
        AddChild(boxInstance);

        boxInstance.AddToGroup("book_boxes");
        boxInstance.GlobalPosition = spawnPoint.GlobalPosition;

        _spawnedBoxes[spawnPoint] = boxInstance;

        // Wanneer box verdwijnt → timer opnieuw starten
        boxInstance.TreeExiting += () =>
        {
            StartSpawnTimer(spawnPoint);
        };
    }
}