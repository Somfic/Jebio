using Jebio.Programs;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace Jebio.Connection;

public class JebioConnection
{
    private readonly ILogger<JebioConnection>? _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly Connector _connector;
    private List<JebioProgram> _programs;
    private bool _isRunning = false;

    public JebioConnection(ILogger<JebioConnection>? log, IServiceProvider serviceProvider, Connector connector)
    {
        _log = log;
        _serviceProvider = serviceProvider;
        _connector = connector;
    }

    public async Task RunAsync()
    {
        Initialise();
        _isRunning = true;
        
        while (_isRunning)
        {
            await Tick();
            
            await Task.Delay(100);
        }
    }

    public async Task StopAsync()
    {
        _isRunning = false;
    }
    
    internal void AddPrograms(IEnumerable<JebioProgram>? programs)
    {
        if(programs == null)
            throw new Exception("No programs to add");

        _programs = programs.ToList();
    }
    
    internal void Connect((string name, string ipAddress, int rpcPort, int streamPort) connection)
    {
        _connector.Connect(connection);
    }

    private void Initialise()
    {
        foreach (var x in _programs) x.Initialise(_serviceProvider);
    }

    private IEnumerable<JebioProgram> FindProgramsToRun()
    {
        return _programs.Where(p => p.IsReadyToRun(_programs));
    }

    private async Task Tick()
    {
        foreach (var program in _programs)
        {
            try
            {
                if (program.IsReadyToRun(_programs) && program.State == ProgramState.Waiting)
                    program.State = ProgramState.Starting;
                
                switch (program.State)
                {
                    case ProgramState.Starting:
                        _log?.LogDebug("Program {Program} started", program.GetType().Name);
                        await program.Start();
                        break;
                    
                    case ProgramState.Running:
                        await program.Tick();
                        break;
                    
                    case ProgramState.Finishing:
                        await program.Finish();
                        _log?.LogDebug("Program {Program} finished", program.GetType().Name);
                        break;
                    
                    case ProgramState.Terminating:
                        _log?.LogDebug(program.Exception, "Program {Program} terminated", program.GetType().Name);
                        await program.Terminate(program.Exception);
                        break;
                }
            }
            catch (Exception ex)
            {
                await program.ReportException(ex);
                _log?.LogDebug(ex, "Error while executing program {Program}", program.GetType().Name);
            }
        }
    }
}