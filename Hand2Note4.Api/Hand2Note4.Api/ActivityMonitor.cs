namespace Hand2Note4.Api;

using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

public class ActivityMonitor : IDisposable {
    private readonly Timer _timer;
    private bool _wasRunning;
    private readonly string _processName = "Hand2Note"; 

    public event EventHandler? Hand2NoteStarted;
    public event EventHandler? Hand2NoteClosed;

    public ActivityMonitor(double checkIntervalMilliseconds = 1000) {
        _wasRunning = IsHand2NoteRunning();
        _timer = new Timer(checkIntervalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void 
    OnTimerElapsed(object? sender, ElapsedEventArgs e) {
        var isRunning = IsHand2NoteRunning();

        if (isRunning && !_wasRunning) {
            _wasRunning = true;
            Hand2NoteStarted?.Invoke(this, EventArgs.Empty);
        }
        else if (!isRunning && _wasRunning) {
            _wasRunning = false;
            Hand2NoteClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsHand2NoteRunning() => Process.GetProcessesByName(_processName).Any();
    public void Dispose() {
        _timer?.Dispose();
    }
}