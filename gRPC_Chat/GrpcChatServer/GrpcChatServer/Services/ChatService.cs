using Grpc.Core;

namespace GrpcChatServer.Services
{
    /// <summary>
    /// Chat service 
    /// </summary>
    public class ChatService : Chat.ChatBase
    {
        /// <summary>
        /// Chat room
        /// </summary>
        private ChatRoom _chatRoom;

        /// <summary>
        /// Initializes class instance of <see cref="ChatService"/>
        /// </summary>
        public ChatService(ChatRoom chatRoom)
        {
            _chatRoom = chatRoom;
        }

        /// <summary>
        /// Sends messages for all in the room (subscribers)
        /// </summary>
        /// <param name="requestStream">Request stream</param>
        /// <param name="responseStream">Response stream</param>
        /// <param name="context">Call context</param>
        /// <returns></returns>
        public override async Task SendMessage(
                IAsyncStreamReader<ChatMessageRequest> requestStream,
                    IServerStreamWriter<ChatMessageServerResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var chatUser = requestStream.Current;

                if (!_chatRoom.IsUserLogedIn(chatUser.User))
                {
                    _chatRoom.Join(chatUser.User, responseStream);
                }                

                /*
                If message is not empty
                */
                if (!string.IsNullOrWhiteSpace(chatUser.Message))
                {
                    var serverResponseMessage = new ChatMessageServerResponse
                    {
                        Message = new ChatMessageRequest
                        {
                            User = chatUser.User,
                            Message = chatUser.Message
                        }
                    };

                    await _chatRoom.PublishMessageAsync(serverResponseMessage);
                }
            }
        }

        /// <summary>
        /// LogIns user to chat session
        /// </summary>
        /// <param name="request">LogIn request</param>
        /// <param name="context">Call context</param>
        /// <returns>Login response</returns>
        public override Task<LoginResponse> LogIn(LoginRequest request, ServerCallContext context)
        {
            var userLogin = request.Login;

            if (string.IsNullOrWhiteSpace(userLogin))
            {
                return Task.FromResult(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "User login cannot be empty!"
                });
            }

            if (_chatRoom.IsUserExists(userLogin))
            {
                return Task.FromResult(new LoginResponse
                {
                    Success = false,
                    ErrorMessage = $"User with '{userLogin}' already exists!"
                });
            }

            _chatRoom.Join(userLogin, null);

            return Task.FromResult(new LoginResponse
            {
                Success = true
            });
        }

        /// <summary>
        /// LogOuts user from chat session
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <param name="context">Call context</param>
        /// <returns>Logout response</returns>
        public override Task<LogoutResponse> LogOut(LogoutRequest request, ServerCallContext context)
        {
            var userLogin = request.Login;

            if (!string.IsNullOrWhiteSpace(userLogin)
                && _chatRoom.IsUserExists(request.Login))
            {
                var r = _chatRoom.Remove(userLogin);
            }

            return Task.FromResult(new LogoutResponse());
        }
    }
}
