using System.Collections.Concurrent;

namespace WebSpark.Main.Utilities
{
    public class ChatHistoryStore
    {
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _chatHistories;

        public ChatHistoryStore()
        {
            _chatHistories = new ConcurrentDictionary<string, List<ChatMessage>>();
        }

        public void AddMessage(string sessionId, ChatMessage message)
        {
            if (!_chatHistories.ContainsKey(sessionId))
            {
                _chatHistories[sessionId] = [];
            }
            _chatHistories[sessionId].Add(message);
        }

        public List<ChatMessage> GetMessages(string sessionId)
        {
            return _chatHistories.ContainsKey(sessionId) ? _chatHistories[sessionId] : new List<ChatMessage>();
        }
    }
}
