using Grpc.Core;
using GrpcApplication;

namespace GrpcApplication.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        await Task.Delay(TimeSpan.FromSeconds(7));

        return new HelloReply
        {
            Message = "Hello " + request.Name
        };
    }
}
