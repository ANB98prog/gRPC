using Grpc.Core;
using Grpc.Net.Client;
using GrpcApplication;
using GrpcGreeterClient;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5231");
        var client = new Customer.CustomerClient(channel);

        try
        {
            var idKey = GetKeyboardKey();

            while (idKey != 'q')
            {
                if (!Int32.TryParse(idKey.ToString(), out int id))
                {
                    Console.WriteLine($"Invalid input. Try again or exit via 'q' key. {Environment.NewLine}");
                    idKey = GetKeyboardKey();
                    continue;
                }

                var customerRequest = new CustomerLookupModel { UserId = id };
                var foundCustomer = await client.GetCustomerInfoAsync(customerRequest);

                if (!string.IsNullOrEmpty(foundCustomer.Id))
                {
                    Console.WriteLine($"Found customer: {foundCustomer.FirstName} {foundCustomer.LastName} Age: {foundCustomer.Age}");
                }
                else
                {
                    Console.WriteLine($"Customer with `{customerRequest.UserId}` is not found");
                }

                idKey = GetKeyboardKey();
            }
           
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            Console.WriteLine("Timeout");
        }

        char GetKeyboardKey()
        {
            Console.WriteLine($"Enter customer id (or 'q' to quit):");
            var key = Console.ReadKey();
            Console.WriteLine(Environment.NewLine);

            return key.KeyChar;
        }
    }
}