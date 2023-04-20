namespace Tgs.StationLetter;

public interface IStationLetterPostman
{
	IAsyncEnumerable<Letter> GetLettersFromMailSlotAsync(CancellationToken cancellationToken = default);

	ValueTask InboxAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default);
}
