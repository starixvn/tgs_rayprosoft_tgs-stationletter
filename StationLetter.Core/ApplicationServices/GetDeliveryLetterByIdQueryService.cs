using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class GetDeliveryLetterByIdQueryService : IGetService<GetDeliveryLetterByIdQuery, DeliveryLetter?>
{
	private readonly ITgsRepository _tgsRepository;

	public GetDeliveryLetterByIdQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<DeliveryLetter?> GetAsync(GetDeliveryLetterByIdQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.GetDeliveryLetterAsync(query.DeliveryId, cancellationToken);
}
