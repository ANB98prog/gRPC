using Grpc.Core;
using Grpc.Net.Client;
using GrpcChatServer;

namespace SimpleConsoleChatClient
{
    internal class Program
    {
        private static bool _exit = false;

        private static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5067");
            var client = new Chat.ChatClient(channel);

            try
            {
                using (var call = client.SendMessage())
                {
                    try
                    {
                        var login = GetUserLogin();

                        if (!_exit)
                        {
                            ListenForMessagesAsync(call);

                            string message = "";

                            while (!message.Equals("exit()"))
                            {
                                Console.Write("$ ");
                                message = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(message))
                                {
                                    var userMessage = new ChatMessageRequest
                                    {
                                        User = login,
                                        Message = message
                                    };

                                    await call.RequestStream.WriteAsync(userMessage);
                                }
                            }
                        }
                    }
                    finally
                    {
                        await call.RequestStream.CompleteAsync();
                        await channel.ShutdownAsync();
                    }
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static string GetUserLogin()
        {
            string login = "";

            while (string.IsNullOrWhiteSpace(login))
            {
                Console.WriteLine("Enter a user login: ");
                login = Console.ReadLine();

                if (login.Equals("exit()"))
                {
                    login = string.Empty;
                    _exit = true;
                }
            }

            return login;
        }

        private static async Task ListenForMessagesAsync(AsyncDuplexStreamingCall<ChatMessageRequest, ChatMessageServerResponse> call)
        {
            // Read messages from the response
            while (await call.ResponseStream.MoveNext())
            {
                var responseMessage = call.ResponseStream.Current;
                Console.WriteLine($"{responseMessage.Message.User}: {responseMessage.Message.Message}");
                Console.Write("$ ");
            }
        }
    }
}