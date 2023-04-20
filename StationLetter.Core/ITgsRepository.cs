namespace Tgs.StationLetter;

public interface ITgsRepository
{
	ValueTask InsertStationLettersAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default);
}
