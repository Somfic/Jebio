using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Jebio;
using Jebio.Programs;
using Microsoft.Extensions.Logging;

namespace Sandbox;

public class PreLaunch : JebioProgram
{
    private readonly ILogger<PreLaunch>? _log;

    public PreLaunch(ILogger<PreLaunch>? log)
    {
        _log = log;
    }

    private int _seconds = 10;
    private readonly Stopwatch counter = new Stopwatch();

    protected override async Task<NextStep> OnTick()
    {
        if (!counter.IsRunning)
        {
            _log?.LogInformation("{Seconds} seconds until launch", _seconds);
            counter.Restart();
        }

        if (counter.ElapsedMilliseconds >= 1000)
        {
            _seconds--;  
            counter.Stop();

            if (_seconds == 0)
            {
                _log?.LogInformation("Launching!");
                return NextStep.Stop;
            }
        }

        return NextStep.Continue;
    }
}