using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Stats : Control
{
    [Export] private Label _timeAlive;
    [Export] private Label _highScore;
    [Export] private Label _npcHelped;

    [Export] private AudioStreamPlayer _showTextSFX;
    [Export] private AudioStreamPlayer _countNumberSFX;

    private float _delayBetween = 0.4f;
    private float _countDuration = 0.8f;

    private async void ShowStats()
    {
        var stats = new List<(Label label, string text, int value)>
        {
            (_timeAlive, "Time alive: ", GameManager._timeAlive),
            (_highScore, "Highscore: ", GameManager._highScore),
            (_npcHelped, "Helped clients: ", GameManager._npcHelped)
        };

        /* clear text */
        foreach(var stat in stats)
            stat.label.Text = "";
        
        /* show text only */
        foreach(var stat in stats)
        {
            stat.label.Text = stat.text;
            await Wait(_delayBetween);
        }

        /* wait */
        await Wait(_delayBetween);

        /* numbers */
        foreach(var stat in stats)
        {
            await CountUp(stat.label, stat.text, stat.value);
            await Wait(_delayBetween);
        }
    }

    private async Task CountUp(Label label, string prefix, int target)
    {
        int current = 0;
        float active = 0f;

        while (active < _countDuration)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            active += (float)GetProcessDeltaTime();
            float t = Mathf.Clamp(active / _countDuration, 0f, 1f);

            current = (int)Mathf.Lerp(0, target, t);
            label.Text = prefix + current.ToString();
            _countNumberSFX.Play();
        }

        label.Text = prefix + target.ToString();
    }

    private async Task Wait(float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
    }
}
