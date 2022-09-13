namespace Jebio.Programs;

public class ProgramRegistrar
{
    private readonly List<Type> _programs = new();
    
    public void AddProgram<T>() where T : JebioProgram
    {
        _programs.Add(typeof(T));
    }

    public IEnumerable<Type> GetPrograms() => _programs.AsReadOnly();
}