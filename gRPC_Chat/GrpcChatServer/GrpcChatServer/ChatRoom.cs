using Grpc.Core;
using System.Collections.Concurrent;

namespace GrpcChatServer
{
    /// <summary>
    /// Chat room
    /// </summary>
    public class ChatRoom
    {
        /// <summary>
        /// Users in the room
        /// </summary>
        private ConcurrentDictionary<string, IServerStreamWriter<ChatMessageServerResponse>> _users 
                        = new ConcurrentDictionary<string, IServerStreamWriter<ChatMessageServerResponse>>();

        /// <summary>
        /// Adds user in the room
        /// </summary>
        /// <param name="name">User login</param>
        /// <param name="responseWriter">User grpc response writer</param>
        public bool Join(string name, IServerStreamWriter<ChatMessageServerResponse> responseWriter)
        {
            return _users.TryAdd(name, responseWriter);
        }

        /// <summary>
        /// Removes user from the room
        /// </summary>
        /// <param name="name">User login</param>
        public void Remove(string name) => _users.TryRemove(name, out var response);

        /// <summary>
        /// Publishes user's message to all the people in the room
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <returns></returns>
        public async Task PublishMessageAsync(ChatMessageServerResponse message)
        {
            foreach (var user in _users)
            {
                await SendMessageToSubscriberAsync(user, message);
            }
        }

        /// <summary>
        /// Sends message to subscriber
        /// </summary>
        /// <param name="user">Subscriber data</param>
        /// <param name="message">Message to publish</param>
        /// <returns></returns>
        private async Task SendMessageToSubscriberAsync(KeyValuePair<string, IServerStreamWriter<ChatMessageServerResponse>> user, ChatMessageServerResponse message)
        {
            try
            {
                await user.Value.WriteAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
