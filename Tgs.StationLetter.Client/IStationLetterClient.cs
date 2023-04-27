namespace Tgs.StationLetterService.Client;

public interface IStationLetterClient
{
	ValueTask<StationLetter?> GetStationLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<StationLetter> ListStationLettersAsync(string receiver, CancellationToken cancellationToken = default);

	ValueTask MarkStationLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask ReplyStationLetterAsync(Guid letterId, ReplyStationLetterRequest request, CancellationToken cancellationToken = default);

	ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default);
}
