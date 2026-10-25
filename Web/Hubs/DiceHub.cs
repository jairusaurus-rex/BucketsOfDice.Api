using Microsoft.AspNetCore.SignalR;
using BucketsOfDice.Api.Common.Enums;
using BucketsOfDice.Api.Messages;
using BucketsOfDice.Api.Services;
using System.Globalization;
using System.Text.Json;

namespace BucketsOfDice.Api.Hubs;

public class DiceHub : Hub
{
    private static readonly JsonSerializerOptions ContentSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly MessageHistory _messageHistory;

    public DiceHub(MessageHistory messageHistory)
    {
        _messageHistory = messageHistory;
    }

    public override async Task OnConnectedAsync()
    {
        var query = Context.GetHttpContext()!.Request.Query;
        var room = Enum.Parse<ChatRoom>(query["room"].ToString());
        var userName = query["userName"].ToString();

        Context.Items[nameof(ChatRoom)] = room;
        Context.Items["UserName"] = userName;

        await Groups.AddToGroupAsync(Context.ConnectionId, room.ToString());

        var joinMessage = CreateEnteredChatMessage(userName);
        _messageHistory.Add(room, joinMessage);
        await Clients.Group(room.ToString()).SendAsync("ReceiveMessage", joinMessage);
        await Clients.Caller.SendAsync("ReceiveHistory", _messageHistory.GetRecent(room));
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(MessageDto message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateMessage(message);

        if (!Context.Items.TryGetValue(nameof(ChatRoom), out var roomValue)
            || roomValue is not ChatRoom room)
            throw new HubException("This connection is not associated with a chat room.");

        var userName = Context.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
            userName = Context.Items.TryGetValue("UserName", out var userNameValue)
                ? userNameValue as string
                : null;
        if (string.IsNullOrWhiteSpace(userName))
            userName = Context.ConnectionId;

        message.UserName = userName;
        _messageHistory.Add(room, message);
        await Clients.Group(room.ToString()).SendAsync("ReceiveMessage", message);
    }

    private static void ValidateMessage(MessageDto message)
    {
        if (string.IsNullOrWhiteSpace(message.Id)
            || !DateTimeOffset.TryParse(message.Timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new HubException("Message id and ISO-8601 timestamp are required.");

        switch (message.Type)
        {
            case "text":
                var textContent = DeserializeContent<TextMessageContentDto>(message.Content);
                if (!string.IsNullOrWhiteSpace(textContent?.Text))
                    return;
                break;
            case "diceRoll":
                var diceContent = DeserializeContent<DiceRollMessageContentDto>(message.Content);
                if (diceContent is null)
                    throw new HubException("Dice-roll content must include rollTitle, dice, result, and bestDice with valid JSON types.");
                if (diceContent.RollTitle is null)
                    throw new HubException("Dice-roll content requires a string rollTitle.");
                if (diceContent.Dice is not { Length: > 0 })
                    throw new HubException("Dice-roll content requires at least one die.");
                for (var index = 0; index < diceContent.Dice.Length; index++)
                {
                    var die = diceContent.Dice[index];
                    if (die is null
                        || string.IsNullOrWhiteSpace(die.Id)
                        || die.Title is null
                        || !double.IsFinite(die.Rank)
                        || !double.IsFinite(die.Result))
                        throw new HubException($"Dice-roll content has an invalid die at index {index}; each die requires a non-empty id, a string title, and finite numeric rank and result values.");
                }
                if (diceContent.BestDice is null)
                    throw new HubException("Dice-roll content requires bestDice to be an array of strings.");
                if (!double.IsFinite(diceContent.Result))
                    throw new HubException("Dice-roll content requires a finite numeric result.");
                return;
            default:
                throw new HubException("Message type is invalid.");
        }

        throw new HubException("Message content is invalid.");
    }

    private static MessageDto CreateEnteredChatMessage(string userName)
    {
        return new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            Type = "text",
            Content = JsonSerializer.SerializeToElement(new { text = $"{userName} has entered the chat" }),
            Timestamp = DateTimeOffset.UtcNow.ToString("O"),
            UserId = userName,
            UserName = userName
        };
    }

    private static T? DeserializeContent<T>(JsonElement content)
    {
        if (content.ValueKind != JsonValueKind.Object)
            return default;

        try
        {
            return content.Deserialize<T>(ContentSerializerOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}