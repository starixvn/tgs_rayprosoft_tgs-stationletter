using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class GetDeliveryLettersByUserQueryService : IQueryService<GetDeliveryLettersByUserQuery, DeliveryLetter>
{
	private readonly ITgsRepository _tgsRepository;

	public GetDeliveryLettersByUserQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public IAsyncEnumerable<DeliveryLetter> QueryAsync(GetDeliveryLettersByUserQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.QueryDeliveryLettersByUserAsync(query.User, cancellationToken);
}
