using System.Net;
using Microsoft.Extensions.Logging;

namespace Jebio.Connection;

public class Connector
{
    private readonly ILogger<Connector>? _log;
    private KRPC.Client.Connection? _connection;

    public Connector(ILogger<Connector>? log)
    {
        _log = log;
    }

    public void Connect((string name, string ipAddress, int rpcPort, int streamPort) connection)
    {
        var (name, ipAddress, rpcPort, streamPort) = connection;

        _log?.LogDebug("Connecting to kRPC server ... ");
        _log?.LogTrace("Name: {Name}, IP: {Ip}, RPC port: {RpcPort}, Stream port: {StreamPort}", name, ipAddress,
            rpcPort, streamPort);

        try
        {
            _connection = new KRPC.Client.Connection(name, IPAddress.Parse(ipAddress), rpcPort, streamPort);
            _log?.LogInformation("Connected to kRPC server");
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Failed to connect to kRPC server");
            throw;
        }
    }

    public void Disconnect()
    {
  
            _log?.LogInformation("Disconnecting from kRPC server");
            _connection?.Dispose();
    }
    
    public KRPC.Client.Connection? GetConnection()
    {
        return _connection;
    }
}