namespace Tgs.StationLetterService;

internal class StationLetterPostman : IStationLetterPostman
{
	private readonly ITgsRepository _tgsRepository;

	public StationLetterPostman(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask InboxAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default)
		=> _tgsRepository.InsertStationLettersAsync(letters, cancellationToken);
}
