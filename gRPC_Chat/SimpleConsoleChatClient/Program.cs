using Grpc.Core;
using Grpc.Net.Client;
using GrpcChatServer;
using System.Drawing;
using System.Runtime.CompilerServices;

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

        /// <summary>
        /// User's login
        /// </summary>
        private static string _userLogin = null;

        /// <summary>
        /// Chat client
        /// </summary>
        private static Chat.ChatClient _client = null;

        private static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5067");
            _client = new Chat.ChatClient(channel);

            try
            {
                using (var call = _client.SendMessage())
                {
                    try
                    {
                        LogInUser();

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
                                        User = _userLogin,
                                        Message = message
                                    };

                                    await call.RequestStream.WriteAsync(userMessage);
                                }
                            }
                        }
                    }
                    finally
                    {
                        LogOutUser();

                        await call.RequestStream.WriteAsync(new ChatMessageRequest
                        {
                            User = _userLogin,
                            Message = "Disconnected"
                        });

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

        /// <summary>
        /// LogIns user to chat room
        /// </summary>
        /// <returns></returns>
        private static void LogInUser()
        {
            var success = false;
            var login = "";

            while (!success
                        && !_exit)
            {
                login = GetUserLogin();

                if (!_exit)
                {
                    var loginResponse = _client.LogIn(new LoginRequest { Login = login });

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

            _userLogin = login;
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
        /// Logouts user
        /// </summary>
        public static void LogOutUser()
        {
            _client.LogOut(new LogoutRequest { Login = _userLogin });
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