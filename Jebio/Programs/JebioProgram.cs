using Jebio.Connection;
using KRPC.Client.Services.Drawing;
using KRPC.Client.Services.InfernalRobotics;
using KRPC.Client.Services.KerbalAlarmClock;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.RemoteTech;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jebio.Programs;

public abstract class JebioProgram
{
    protected KRPC.Client.Services.KRPC.Service KRPC;
    protected KRPC.Client.Services.SpaceCenter.Service SpaceCenter;
    protected KRPC.Client.Services.Drawing.Service Drawing;
    protected KRPC.Client.Services.InfernalRobotics.Service InfernalRobotics;
    protected KRPC.Client.Services.RemoteTech.Service RemoteTech;
    protected KRPC.Client.Services.UI.Service UI;
    protected KRPC.Client.Services.KerbalAlarmClock.Service KerbalAlarmClock;

    internal Exception Exception { get; set; }

    internal void Initialise(IServiceProvider services)
    {
        var log = services.GetService<ILogger<JebioProgram>>();
        var connector = services.GetRequiredService<Connector>();
        var connection = connector.GetConnection();

        if (connection == null)
        {
            log?.LogError("Cannot initialise program, no connection");
            return;
        }

        KRPC = connection.KRPC();
        SpaceCenter = connection.SpaceCenter();
        Drawing = connection.Drawing();
        InfernalRobotics = connection.InfernalRobotics();
        RemoteTech = connection.RemoteTech();
        UI = connection.UI();
        KerbalAlarmClock = connection.KerbalAlarmClock();
    }
    
    public ProgramState State { get; internal set; } = ProgramState.Waiting;
    
    public virtual bool IsReadyToRun(IEnumerable<JebioProgram> programs) => true;

    internal async Task Start()
    {
        var nextStep = await OnStart();

        State = nextStep == NextStep.Continue ? ProgramState.Running : ProgramState.Finishing;
    }

    protected virtual Task<NextStep> OnStart() => Task.FromResult(NextStep.Continue);

    internal async Task Tick()
    {
        var nextStep = await OnTick();
        
        if (nextStep == NextStep.Stop)
            State = ProgramState.Finishing;
    }
    
    protected abstract Task<NextStep> OnTick();

    public async Task Finish()
    {
        await OnFinished();
        State = ProgramState.Stopped;
    }

    protected virtual Task OnFinished() => Task.CompletedTask;

    public async Task Terminate(Exception ex)
    { 
        await OnTerminated(ex);
        State = ProgramState.Stopped;
    }

    protected virtual Task OnTerminated(Exception ex) => Task.CompletedTask;

    internal async Task ReportException(Exception exception)
    {
        Exception = exception;
        
        var nextStep = await OnException(exception);
        
        if(nextStep == NextStep.Stop)
            State = ProgramState.Terminating;
    }

    protected virtual Task<NextStep> OnException(Exception ex) => Task.FromResult(NextStep.Stop);
}