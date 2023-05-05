namespace Tgs.StationLetterService;

public interface ITgsRepository
{
	ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask InsertStationLettersAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default);

	ValueTask SetLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask SetLetterRepliedAsync(Guid letterId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<Letter> QueryLettersByUserAsync(string user, CancellationToken cancellationToken = default);

	ValueTask InsertDeliveryLetterAsync(DeliveryLetter deliveryLetter, CancellationToken cancellationToken = default);

	ValueTask<DeliveryLetter?> GetDeliveryLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DeliveryLetter> QueryDeliveryLettersByUserAsync(string user, CancellationToken cancellationToken = default);
}
