namespace Tgs.StationLetter;

public interface ITgsRepository
{
	ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask InsertStationLettersAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default);

	ValueTask SetLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask SetLetterRepliedAsync(Guid letterId, CancellationToken cancellationToken = default);
}
