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
        private ConcurrentDictionary<string, IServerStreamWriter<ChatMessageServerResponse>?> _users 
                        = new ConcurrentDictionary<string, IServerStreamWriter<ChatMessageServerResponse>?>();

        /// <summary>
        /// Adds user in the room
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="responseWriter">User grpc response writer</param>
        public void Join(string login, IServerStreamWriter<ChatMessageServerResponse> responseWriter)
        {
            if (!_users.ContainsKey(login))
            {
                _users.TryAdd(login, responseWriter);
            }
            else if(_users.TryGetValue(login, out var writer))
            {
                _users.TryUpdate(login, responseWriter, null);
            }
        }

        /// <summary>
        /// Checks is user loged in 
        /// </summary>
        /// <param name="login">User login</param>
        /// <returns></returns>
        public bool IsUserLogedIn(string login)
        {
            if (_users.TryGetValue(login, out var responseWriter))
            {
                if (responseWriter != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is user exists
        /// </summary>
        /// <param name="login">User login</param>
        /// <returns>
        /// true if exists else false
        /// </returns>
        public bool IsUserExists(string login)
        {
            return _users.ContainsKey(login);
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
