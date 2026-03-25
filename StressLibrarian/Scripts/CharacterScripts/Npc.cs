using Godot;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Transactions;

public partial class Npc : CharacterBody3D
{
    [Export] private MeshInstance3D _DELETEME;

    public enum NPCState
    {
        IDLE,
        WANDER,
        LOUD,
        SNIFFPLAYER,
        ASKPLAYER
    }

    private enum TutorialStep
    {
        NONE,
        LOUD,
        EXPLAIN,
        ASK,
        FOLLOW,
        DONE
    }

    private TutorialStep _tutorialStep = TutorialStep.NONE;
    private bool _isTutorial = false;
    private bool _tutorialBusy = false;

    private string[] sentences =
    {
        "Darling, where can I find the {RequestedGenre} books?",
        "WHERE. IS. {RequestedGenre}?",
        "Where can I find {RequestedGenre}?",
        "Please, help me find {RequestedGenre}.",
        "I'm looking for {RequestedGenre}!",
        "WHERE THE F.. FLIP IS {RequestedGenre}?",
        "I've been looking EVERYWHERE for {RequestedGenre}!"
    };

    private string[] tutorialDialogue =
    {
        "I haven't seen you before...",
        "So you're the new librarian.",
        "Listen carefully.",

        "Customers can get loud.",
        "When they do, silence them with [E].",

        "Some customers will get lost.",
        "Guide them to the correct bookshelf.",

        "Helping them decreases stress.",
        "Too much stress... and you're done.",

        "Keep the library quiet.",
        "Keep it organized.",

        "Let's try it."
    };

    private int _dialogueIndex = 0;
    private bool _isInDialogue = false;


    [Export] private NavigationAgent3D _navAgent;
    [Export] private NavigationRegion3D _navRegion;
    [Export] private Node3D _models;
    [Export] private int _chosenModel;
    [Export] private RayCast3D _raycastNpc;
    [Export] private float _sniffMinRadius = 3f;
    [Export] private float _sniffMaxRadius = 6f;
    [Export] private float _askStopDistance = 1.5f;
    private float _askCooldown = 0f;
    private bool _isFollowingPlayer = false;
    private bool _isAsking = false;

    private NPCState _state;
    private CharacterBody3D _player;
    private float _actionCooldown;
    [Export] private float _baseCooldown = 5f;
    [Export] private float _minCooldown = 5f;
    private float _spawnLock = 0f;

    private float _askTimer = 0f;
    [Export] private float _askTimerMax = 16f;
    private float _loudExtraStress = 0f;

    [Export] private AnimationPlayer _askAnimation;

    [Export] private AudioStreamPlayer3D _loudSFX;

    private Vector3 _wanderTarget;

    private float _moveSpeed = 6f;
    private float _sprintSpeed = 9f;

    public BookGenre RequestedGenre;
    [Export] private Label _requestedGenreLabel;
    private bool _playerReachedCorrectBookshelf = false;



    public override void _Ready()
    {
        if (GetTree().CurrentScene.Name == "Tutorial")
        {
            _isTutorial = true;
        }

        GetRandomModel();

        _spawnLock = (float)GD.RandRange(30f, 45f);

        _state = NPCState.IDLE;
        _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        _actionCooldown = (float)GD.RandRange(1f, 6f);
    }


    /* -------- */
    /* TUTORIAL */
    /* -------- */
    public void StartTutorial()
    {
        _tutorialStep = TutorialStep.LOUD;
        SetState(NPCState.LOUD);
    }

    /*  EXPLAIN LOUD
        LOUD
        EXPLAIN ASK
        ASK
        EXPLAIN STRESS
        DIE*/
    private void RunTutorial(double delta)
    {
        if (_tutorialStep == TutorialStep.DONE)
            return;

        switch (_tutorialStep)
        {
            case TutorialStep.LOUD:
                DoLoud(delta);
                break;

            case TutorialStep.EXPLAIN:
                break;

            case TutorialStep.ASK:
                if (_isFollowingPlayer)
                    AskFollowPlayerToBookshelf(delta);
                else
                    DoMoveToPlayer(delta);
                break;
        }
    }

    private async void ExplainStress()
    {
        _dialogueIndex = 0;
        _isInDialogue = true;

        if (_player is Player player)
        {
            Visible = false;
            player.StartNpcInteraction(this);
        }
    }

    private void ShowCurrentDialogue()
    {
        _requestedGenreLabel.Visible = true;
        _requestedGenreLabel.Text = tutorialDialogue[_dialogueIndex] + "\n\n[E]";
    }

    private void StartDialogue()
    {
        _dialogueIndex = 0;
        _isInDialogue = true;

        if (_player is Player player)
        {
            Visible = false;
            player.StartNpcInteraction(this);
        }

        ShowCurrentDialogue();
    }

    private void EndDialogue()
    {
        _isInDialogue = false;
        _isFollowingPlayer = false;
        _isAsking = false;

        _navAgent.TargetPosition = Vector3.Zero;

        if (_player is Player player)
        {
            Visible = true;
            player.EndNpcInteraction();
        }

        _requestedGenreLabel.Visible = false;

        _tutorialStep = TutorialStep.ASK;
        SetState(NPCState.ASKPLAYER);
    }

    /* -------------------- */
    /* EVERY FUNCTION LOLOL */
    /* -------------------- */
    private void ExtraStress()
    {
        if (_state == NPCState.LOUD)
        {
            _loudExtraStress++;
            if (_loudExtraStress >= 2)
            {
                GameManager.Stress++;
                _loudExtraStress = 0;
            }
        }

        if (_state == NPCState.ASKPLAYER && _isFollowingPlayer)
        {
            GameManager.Stress += 1;
        }
    }

    private void MoveNPC(double delta)
    {
        if (_navAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 next = _navAgent.GetNextPathPosition();
        Vector3 direction = (next - GlobalPosition).Normalized();

        if (direction.Length() > 0.01f)
            Rotation = new Vector3(0, (float)Math.Atan2(direction.X, direction.Z), 0);

        Velocity = direction * _moveSpeed;
        MoveAndSlide();
    }

    private void GetRandomModel()
    {
        int count = _models.GetChildCount();
        int chosen = GD.RandRange(0, count - 1);

        for (int i = 0; i < count; i++)
        {
            Node3D model = _models.GetChild<Node3D>(i);
            model.Visible = i == chosen;

            if (i != chosen)
                model.QueueFree();
        }

        _chosenModel = chosen;
    }

    public int GetChosenModel()
    {
        return _chosenModel;
    }

    private void ResetCooldown()
    {
        _actionCooldown = (float)GD.RandRange(_baseCooldown, _baseCooldown * 2.5f) + GD.Randf() * 2;
    }

    private void ChooseNextAction()
    {
        NPCState newState;

        do
        {
            newState = (NPCState)GD.RandRange(0, 4);

            if (newState == NPCState.LOUD &&
            (GameManager.ActiveLoudNPC >= GameManager.ActiveLoudNPCMax
            || GameManager.LoudGlobalCooldown > 0
            || GameManager.LoudSpawnDelay > 0))
                continue;

            if (newState == NPCState.ASKPLAYER &&
            (GameManager.ActiveAskNPC >= GameManager.ActiveAskNPCMax
            || _askCooldown > 0
            || GameManager.AskGlobalCooldown > 0))
                continue;

            break;

        } while (true);

        SetState(newState);
    }

    private void SetState(NPCState newState)
    {
        if (_isTutorial && _tutorialStep == TutorialStep.DONE)
            return;

        /* exit old states */
        if (_state == NPCState.LOUD)
        {
            GameManager.ActiveLoudNPC--;
            if (_loudSFX.Playing == true)
                _loudSFX.Playing = false;
        }

        if (_state == NPCState.ASKPLAYER)
            GameManager.ActiveAskNPC--;


        /* enter new state */
        if (newState == NPCState.LOUD)
        {
            GameManager.ActiveLoudNPC++;
            GameManager.LoudSpawnDelay = (float)GD.RandRange(3f, 11f);
        }

        if (newState == NPCState.ASKPLAYER)
        {
            GameManager.ActiveAskNPC++;
            _askTimer = _askTimerMax;
        }

        _state = newState;
        ResetCooldown();
    }

    private Vector3 GetRandomNavLocation()
    {
        if (_navRegion == null || _navRegion.NavigationMesh == null)
            return GlobalPosition;

        var vertices = _navRegion.NavigationMesh.GetVertices();

        if (vertices.Length == 0)
            return GlobalPosition;

        int index = GD.RandRange(0, vertices.Length - 1);
        return vertices[index];
    }

    private Vector3 GetRandomPointAroundPlayer()
    {
        if (_player == null)
            return GlobalPosition;

        float angle = GD.Randf() * Mathf.Tau;
        float distance = Mathf.Lerp(_sniffMinRadius, _sniffMaxRadius, GD.Randf());
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        return _player.GlobalPosition + offset;
    }

    private async void AskOnPlayerReached()
    {
        _isAsking = true;

        _askTimer = _askTimerMax;

        if (_player is Player player)
        {
            Visible = false;
            player.StartNpcInteraction(this);
        }

        /* NPC asks player where to go */
        if (_isTutorial)
        {
            RequestedGenre = BookGenre.Action;
        }
        else
        {
            RequestedGenre = (BookGenre)GD.RandRange(0, Enum.GetValues(typeof(BookGenre)).Length - 1);
        }

        if (_requestedGenreLabel.Visible == false)
        {
            _requestedGenreLabel.Visible = true;
            GameManager.Stress -= 5;
        }


        int index = (int)GD.RandRange(0, sentences.Length - 1);
        string chosen = sentences[index];
        _requestedGenreLabel.Text = chosen.Replace("{RequestedGenre}", RequestedGenre.ToString().ToUpper());

        _askAnimation.Play("Play");

        await ToSignal(_askAnimation, AnimationPlayer.SignalName.AnimationFinished);
        _isAsking = false;
        _isFollowingPlayer = true;

        if (_player is Player player2)
        {
            Visible = true;
            player2.EndNpcInteraction();
        }
    }



    private void AskFollowPlayerToBookshelf(double delta)
    {
        if (_player == null || !_isFollowingPlayer)
            return;

        _navAgent.TargetPosition = _player.GlobalPosition;

        MoveNPC(delta);

        if (_playerReachedCorrectBookshelf)
        {
            _isFollowingPlayer = false;
            _playerReachedCorrectBookshelf = false;
            _askCooldown = 10f;

            if (_isTutorial)
            {
                _tutorialStep = TutorialStep.DONE;

                ((Tutorial)GetTree().CurrentScene).EndTutorial();

                _requestedGenreLabel.Text = "Thanks, head on to the last door. And goodluck!\n\n[E]";
            }

            SetState(NPCState.IDLE);
        }
    }

    public void OnPlayerReachedCorrectShelf(BookGenre genre)
    {
        if (_state != NPCState.ASKPLAYER)
            return;

        if (genre == RequestedGenre)
        {
            _playerReachedCorrectBookshelf = true;
            _requestedGenreLabel.Visible = false;

            GameManager._npcHelped++;
            GameManager.Stress -= 25;
            GameManager.AskGlobalCooldown = (float)GD.RandRange(16f, 40f);
        }
    }

    private void DoMoveToTarget(double delta) /* long live AI */
    {
        if (_wanderTarget == Vector3.Zero || GlobalPosition.DistanceTo(_wanderTarget) < 0.5f)
        {
            _wanderTarget = GetRandomNavLocation();
            _navAgent.TargetPosition = _wanderTarget;
        }

        MoveNPC(delta);

        if (_navAgent.IsNavigationFinished())
        {
            SetState(NPCState.IDLE);
        }
    }

    private void DoLoud(double delta)
    {
        _DELETEME.Visible = true;
        if (_loudSFX.Playing == false)
            _loudSFX.Playing = true;
    }

    private void DoSniffPlayer(double delta)
    {

        if (_player == null)
            return;

        if (_navAgent.IsNavigationFinished() || _navAgent.TargetPosition == Vector3.Zero)
            _navAgent.TargetPosition = GetRandomPointAroundPlayer();

        MoveNPC(delta);

        if (_navAgent.IsNavigationFinished())
        {
            SetState(NPCState.IDLE);
        }
    }

    private void DoMoveToPlayer(double delta)
    {
        if (_isTutorial && _tutorialStep == TutorialStep.DONE)
            return;

        if (_player == null)
            return;

        _navAgent.TargetPosition = _player.GlobalPosition;

        MoveNPC(delta);

        if (!_isFollowingPlayer && !_isAsking && GlobalPosition.DistanceTo(_player.GlobalPosition) <= _askStopDistance)
        {
            AskOnPlayerReached();
        }
    }

    public void Interact(Player player)
    {
        if (_isTutorial)
        {
            if (_tutorialStep == TutorialStep.LOUD)
            {
                _loudSFX.Playing = false;
                SetState(NPCState.IDLE);

                _tutorialStep = TutorialStep.EXPLAIN;
                StartDialogue();
                return;
            }
        }

        if (_isTutorial && _tutorialStep == TutorialStep.EXPLAIN)
        {
            if (_isInDialogue)
            {
                _dialogueIndex++;

                if (_dialogueIndex >= tutorialDialogue.Length)
                {
                    EndDialogue();
                }
                else
                {
                    ShowCurrentDialogue();
                }
            }
            return;
        }


        switch (_state)
        {
            case NPCState.LOUD:
                SetState(NPCState.IDLE);
                _loudSFX.Playing = false;
                _DELETEME.Visible = false;

                GameManager.Stress -= 7;
                GameManager._npcShushed++;
                GameManager.LoudGlobalCooldown = (float)GD.RandRange(15f, 35f);

                break;

            default:
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().CurrentScene.Name == "Tutorial")
        {
            RunTutorial(delta);
            return;
        }


        if (_spawnLock > 0f)
        {
            _spawnLock -= (float)delta;
        }
        else
        {
            _actionCooldown -= (float)delta;
        }

        if (_askCooldown > 0f)
            _askCooldown -= (float)delta;

        if ((_state == NPCState.IDLE || _state == NPCState.WANDER) && _actionCooldown <= 0)
        {
            ChooseNextAction();
        }

        if (_state == NPCState.ASKPLAYER && !_isFollowingPlayer)
        {
            _askTimer -= (float)delta;

            if (_askTimer <= 0)
            {
                GameManager.AskGlobalCooldown = (float)GD.RandRange(14f, 30f);
                SetState(NPCState.IDLE);
            }
        }

        switch (_state)
        {
            case NPCState.IDLE:
                break;

            case NPCState.WANDER:
                DoMoveToTarget(delta);
                break;
            case NPCState.LOUD:
                DoLoud(delta);
                break;
            case NPCState.SNIFFPLAYER:
                DoSniffPlayer(delta);
                break;
            case NPCState.ASKPLAYER:
                if (_isFollowingPlayer)
                    AskFollowPlayerToBookshelf(delta);
                else
                    DoMoveToPlayer(delta);
                break;
        }
    }
}
