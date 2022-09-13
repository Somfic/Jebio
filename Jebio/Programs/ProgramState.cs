namespace Jebio.Programs;

public enum ProgramState
{
    Waiting,
    Starting,
    Running,
    Finishing,
    Terminating,
    Stopped
}

public enum NextStep
{
    Continue,
    Stop
}