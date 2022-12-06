using Grpc.Core;
using Grpc.Net.Client;
using GrpcApplication;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5231");
        var client = new Customer.CustomerClient(channel);

        try
        {
            using (var call = client.GetNewCustomers(new NewCustomerRequest()))
            {
                Console.WriteLine("New Customers List:");
                Console.WriteLine();

                while (await call.ResponseStream.MoveNext())
                {
                    var customer = call.ResponseStream.Current;
                    Console.WriteLine($"Customer: {customer.FirstName} {customer.LastName} {customer.EmailAddress}");
                }
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            Console.WriteLine("Timeout");
        }
    }
}