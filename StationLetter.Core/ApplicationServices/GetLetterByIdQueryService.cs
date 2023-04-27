using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class GetLetterByIdQueryService : IGetService<GetLetterByIdQuery, Letter?>
{
	private readonly ITgsRepository _tgsRepository;

	public GetLetterByIdQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<Letter?> GetAsync(GetLetterByIdQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.GetLetterAsync(query.LetterId, cancellationToken);
}
