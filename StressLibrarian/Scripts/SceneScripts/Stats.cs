using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Stats : Control
{
    [Export] private Label _timeAlive;
    [Export] private Label _highScore;
    [Export] private Label _npcShushed;
    [Export] private Label _npcHelped;

    [Export] private AudioStreamPlayer _showTextSFX;
    [Export] private AudioStreamPlayer _countNumberSFX;

    private float _delayBetween = 0.4f;
    private float _countDuration = 0.8f;


    public override void _Ready()
    {
        _timeAlive.Text = "";
        _highScore.Text = "";
        _npcShushed.Text = "";
        _npcHelped.Text = "";
    }


    private async void ShowStats()
    {
        var stats = new List<(Label label, string text, int value, bool isTime)>
        {
            (_timeAlive, "Time alive: ", GameManager._timeAlive, true),
            (_highScore, "Highscore: ", GameManager._highScore, false),
            (_npcShushed, "Shushed clients: ", GameManager._npcShushed, false),
            (_npcHelped, "Helped clients: ", GameManager._npcHelped, false)
        };

        /* clear text */
        foreach (var stat in stats)
            stat.label.Text = "";

        /* show text only */
        foreach (var stat in stats)
        {
            stat.label.Text = stat.text;
            await Wait(_delayBetween);
        }

        /* wait */
        await Wait(_delayBetween);

        /* numbers */
        foreach (var stat in stats)
        {
            await CountUp(stat.label, stat.text, stat.value, stat.isTime);
            await Wait(_delayBetween);
        }
    }

    private async Task CountUp(Label label, string prefix, int target, bool isTime)
    {
        int current = 0;
        float active = 0f;

        while (active < _countDuration)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            active += (float)GetProcessDeltaTime();
            float t = Mathf.Clamp(active / _countDuration, 0f, 1f);

            current = (int)Mathf.Lerp(0, target, t);

            if (isTime)
            {
                label.Text = prefix + TimeSpan.FromSeconds(current).ToString(@"hh\:mm\:ss");
            }
            else
            {
                label.Text = prefix + current.ToString();
            }

            _countNumberSFX.Play();
        }

        if (isTime)
        {
            label.Text = prefix + TimeSpan.FromSeconds(current).ToString(@"hh\:mm\:ss");
        }
        else
        {
            label.Text = prefix + current.ToString();
        }
    }

    private async Task Wait(float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
    }
}
