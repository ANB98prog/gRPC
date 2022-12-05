using Grpc.Core;
using Grpc.Net.Client;
using GrpcGreeterClient;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5231");
        var client = new Greeter.GreeterClient(channel);

        try
        {
            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "GreeterClient" },
                deadline: DateTime.UtcNow.AddSeconds(8)
            );

            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            Console.WriteLine("Timeout");
        }
    }
}