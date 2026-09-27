using System.Text.Json;
using System.Text.Json.Serialization;

namespace BucketsOfDice.Api.Messages;

public sealed class MessageDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("content")]
    public required JsonElement Content { get; set; }

    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; set; }

    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

}

public sealed class TextMessageContentDto
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public sealed class DiceRollMessageContentDto
{
    [JsonPropertyName("rollTitle")]
    public required string RollTitle { get; set; }

    [JsonPropertyName("dice")]
    public required DiceDto[] Dice { get; set; }

    [JsonPropertyName("result")]
    public required double Result { get; set; }

    [JsonPropertyName("bestDice")]
    public required string[] BestDice { get; set; }
}

public sealed class DiceDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("rank")]
    public required double Rank { get; set; }

    [JsonPropertyName("result")]
    public required double Result { get; set; }
}