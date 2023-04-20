namespace Tgs.StationLetter;

public interface IStationLetterPostman
{
	ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask InboxAsync(Letter letter, CancellationToken cancellationToken = default);
}
