namespace Tgs.StationLetterService.Client;

public interface IStationLetterClient
{
	ValueTask<StationLetter?> GetStationLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<StationLetter> ListStationLettersAsync(string receiver, CancellationToken cancellationToken = default);

	ValueTask MarkStationLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask ReplyStationLetterAsync(Guid letterId, ReplyStationLetterRequest request, CancellationToken cancellationToken = default);

	ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default);

	ValueTask PostStationLetterAsync(PostStationLetterRequest request, CancellationToken cancellationToken = default);

	ValueTask SendBrandAllPlayersStationLetterAsync(BrandAllPlayersStationLetterRequest request, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DeliveryLetter> ListDeliverLettersAsync(string sender, CancellationToken cancellationToken = default);

	ValueTask<DeliveryLetter?> GetDeliveryLetterAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}
