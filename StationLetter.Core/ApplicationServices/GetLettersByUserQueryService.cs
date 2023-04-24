using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class GetLettersByUserQueryService : IQueryService<GetLettersByUserQuery, Letter>
{
	private readonly ITgsRepository _tgsRepository;

	public GetLettersByUserQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public IAsyncEnumerable<Letter> QueryAsync(GetLettersByUserQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.QueryLettersByUserAsync(query.User, cancellationToken);
}
