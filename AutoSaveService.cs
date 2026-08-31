using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace AdvancedNotepad.App;

public class AutoSaveService
{
    private Timer? _timer;
    private readonly SessionStateService _session = new();

    public void Start(IEnumerable<EditorTabViewModel> tabs, TimeSpan interval)
    {
        _timer = new Timer(interval.TotalMilliseconds);
        _timer.Elapsed += (_, _) =>
        {
            foreach (var t in tabs.Where(t => t.IsDirty))
                _session.PersistTab(t);
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    public void Stop() => _timer?.Stop();
}