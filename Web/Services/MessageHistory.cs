using BucketsOfDice.Api.Common.Enums;
using BucketsOfDice.Api.Messages;

namespace BucketsOfDice.Api.Services;

public sealed class MessageHistory
{
    private const int MaxMessagesPerRoom = 50;
    private readonly object _lock = new();
    private readonly Dictionary<ChatRoom, Queue<MessageDto>> _messages = new();

    public void Add(ChatRoom room, MessageDto message)
    {
        lock (_lock)
        {
            if (!_messages.TryGetValue(room, out var queue))
                _messages[room] = queue = new Queue<MessageDto>();

            if (queue.Count == MaxMessagesPerRoom)
                queue.Dequeue();

            queue.Enqueue(message);
        }
    }

    public MessageDto[] GetRecent(ChatRoom room)
    {
        lock (_lock)
            return _messages.TryGetValue(room, out var queue)
                ? queue.ToArray()
                : Array.Empty<MessageDto>();
    }
}