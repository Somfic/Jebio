using Jebio.Programs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Valsom.Logging.PrettyConsole;

namespace Jebio.Connection;

public class JebioConnectionBuilder
{
    private (string name, string ipAddress, int rpcPort, int streamPort) _connection;
    private readonly IHostBuilder _builder =  Host.CreateDefaultBuilder();
    private ProgramRegistrar _registrar = new();

    internal JebioConnectionBuilder(string ipAddress, int rpcPort, int streamPort)
    {
        _connection = ("Jebio", ipAddress, rpcPort, streamPort);

        _builder.ConfigureServices(services =>
            {
                services.AddSingleton<Connector>();
                services.AddSingleton<JebioConnection>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Trace);
            });
    }

    public static JebioConnectionBuilder Create(string ipAddress = "127.0.0.1", int rpcPort = 50000, int streamPort = 50001)
    {
        return new JebioConnectionBuilder(ipAddress, rpcPort, streamPort);
    }

    public JebioConnectionBuilder WithName(string name)
    {
        _connection.name = name;
        return this;
    }

    public JebioConnectionBuilder WithProgram<T>() where T : JebioProgram
    {
        _registrar.AddProgram<T>();
        return this;
    }
    
    public Task RunAsync()
    {
        var connection = Build();
        return connection.RunAsync();
    }
    
    private JebioConnection Build()
    {
        var host = _builder.Build();
        var app =  host.Services.GetRequiredService<JebioConnection>();
        
        if (app == null)
            throw new Exception("Could not build application");
        
        app.AddPrograms(
            _registrar.GetPrograms()
            .Select(t => ActivatorUtilities.CreateInstance(host.Services, t))
            .Cast<JebioProgram>());

        app.Connect(_connection);

        return app;
    }
}