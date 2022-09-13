using Jebio.Programs;

namespace Sandbox;

public class Launch : JebioProgram
{
    public override bool IsReadyToRun(IEnumerable<JebioProgram> programs)
    {
        return programs.Any(p => p is PreLaunch && p.State == ProgramState.Stopped);
    }

    protected override async Task<NextStep> OnTick()
    {
        SpaceCenter.ActiveVessel.Control.Throttle = 1;
        
        if (SpaceCenter.ActiveVessel.AvailableThrust == 0) {}
            SpaceCenter.ActiveVessel.Control.ActivateNextStage();

            return NextStep.Continue;
    }
}