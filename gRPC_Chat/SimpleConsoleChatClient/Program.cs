using Grpc.Core;
using Grpc.Net.Client;
using GrpcChatServer;
using System.Drawing;

namespace SimpleConsoleChatClient
{
    internal class Program
    {
        /// <summary>
        /// Application state
        /// </summary>
        private static bool _exit = false;

        /// <summary>
        /// Command to exit from the chat
        /// </summary>
        private static readonly string _exitCommand = "quit";

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
                        var login = LogInUser(client);

                        if (!_exit)
                        {
                            ListenForMessagesAsync(call);

                            string message = "";

                            while (!message.Equals(_exitCommand))
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
                    }
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// LogIns user to chat room
        /// </summary>
        /// <param name="client">Chat client</param>
        /// <returns></returns>
        private static string LogInUser(Chat.ChatClient client)
        {
            var success = false;
            var login = "";

            while (!success
                        && !_exit)
            {
                login = GetUserLogin();

                if (!_exit)
                {
                    var loginResponse = client.LogIn(new LoginRequest { Login = login });

                    if (loginResponse.Success)
                    {
                        success = true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(loginResponse.ErrorMessage);
                        Console.ResetColor();
                    } 
                }
            }

            return login;
        }

        /// <summary>
        /// Gets user's login
        /// </summary>
        /// <returns></returns>
        private static string GetUserLogin()
        {
            string login = "";

            while (string.IsNullOrWhiteSpace(login)
                && !_exit)
            {
                Console.WriteLine("Enter a user login: ");
                login = Console.ReadLine();

                if (login.Equals(_exitCommand))
                {
                    login = string.Empty;
                    _exit = true;
                }
            }

            return login;
        }

        /// <summary>
        /// Listens for a messages
        /// </summary>
        /// <param name="call">Chat client session</param>
        /// <returns></returns>
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