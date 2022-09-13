using Jebio;
using Jebio.Connection;
using Sandbox;

await JebioConnectionBuilder
    .Create("192.168.2.18")
    .WithProgram<PreLaunch>()
    .WithProgram<Launch>()
    .RunAsync();