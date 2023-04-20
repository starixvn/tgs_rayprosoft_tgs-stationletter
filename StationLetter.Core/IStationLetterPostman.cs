namespace Tgs.StationLetter;

public interface IStationLetterPostman
{
	ValueTask InboxAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default);
}
