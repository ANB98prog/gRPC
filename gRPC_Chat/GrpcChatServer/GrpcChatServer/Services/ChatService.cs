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
            try
            {
                if (!await requestStream.MoveNext()) return;

                do
                {
                    var chatUser = requestStream.Current;

                    _chatRoom.Join(chatUser.User, responseStream);

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
                } while (await requestStream.MoveNext());
            }
            catch (IOException ex)
            {
                Console.WriteLine("Client disconnected...");
            }
        }
    }
}
