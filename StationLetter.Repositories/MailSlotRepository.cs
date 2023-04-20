using System.Text.Json;
using StackExchange.Redis;

namespace Tgs.StationLetter.Repositories;

internal class MailSlotRepository : IMailSlotRepository
{
	private static readonly RedisKey _KeyPrefix = new("Tgs:StationLetter");
	private static readonly JsonSerializerOptions _SerializerOptions = new(JsonSerializerDefaults.Web);

	private readonly IDatabase _redisDatabase;

	public MailSlotRepository(IDatabase redisDatabase)
	{
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
	}

	public async ValueTask<Guid> SaveSendLetterAsync(SendLetter sendLetter, CancellationToken cancellationToken = default)
	{
		var id = Guid.NewGuid();
		var key = _KeyPrefix.Append(id.ToString());

		_ = await _redisDatabase.StringSetAsync(
			key,
			JsonSerializer.Serialize(sendLetter, _SerializerOptions)).ConfigureAwait(false);

		return id;
	}

	public async ValueTask<SendLetter?> GetSendLetterAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var key = _KeyPrefix.Append(id.ToString());

		var json = await _redisDatabase.StringGetAsync(key).ConfigureAwait(false);

		return JsonSerializer.Deserialize<SendLetter>((string?)json ?? string.Empty, _SerializerOptions);
	}

	public async ValueTask SaveLetterAsync(Guid id, Letter letter, CancellationToken cancellationToken = default)
	{
		var key = _KeyPrefix.Append("MailSlot").Append(id.ToString());

		_ = await _redisDatabase.ListRightPushAsync(key, JsonSerializer.Serialize(letter, _SerializerOptions)).ConfigureAwait(false);
	}
}
