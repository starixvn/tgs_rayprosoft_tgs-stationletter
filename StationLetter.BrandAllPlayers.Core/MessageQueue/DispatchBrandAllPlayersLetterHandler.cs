using Tgs.BrandCertificate.Clients;
using Tgs.MessageQueue;
using Tgs.PlayerService.Client;
using Tgs.StationLetter.Protos;
using Tgs.StationLetterService.Client;

namespace Tgs.StationLetter.BrandAllPlayers.MessageQueue;

internal class DispatchBrandAllPlayersLetterHandler : IMessageHandler
{
	private readonly IStationLetterClient _stationLetterClient;
	private readonly IPlayerServiceClient _playerServiceClient;
	private readonly IBrandServiceClient _brandServiceClient;

	public DispatchBrandAllPlayersLetterHandler(
		IStationLetterClient stationLetterClient,
		IPlayerServiceClient playerServiceClient,
		IBrandServiceClient brandServiceClient)
	{
		_stationLetterClient = stationLetterClient ?? throw new ArgumentNullException(nameof(stationLetterClient));
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
		_brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = DeliverLetterRequest.Parser.ParseFrom(data.Span);

		var deliveryLetter = await _stationLetterClient.GetDeliveryLetterAsync(
			request.DeliverLetterId,
			cancellationToken).ConfigureAwait(false);

		if (deliveryLetter is null)
			return;

		var brandCode = deliveryLetter.Sender;

		var brand = await _brandServiceClient.GetBrandInfoByCodeAsync(
			brandCode,
			cancellationToken).ConfigureAwait(false);

		if (brand is null)
			return;

		await _playerServiceClient.ListPlayerCodesByBrandIdAsync(brand.Id, cancellationToken)
			.Buffer(1000)
			.ForEachAwaitAsync(
				async list =>
				{
					var request = new PostStationLetterRequest(
						deliveryLetter.Sender,
						list,
						deliveryLetter.Subject,
						deliveryLetter.Content,
						deliveryLetter.ComeFrom,
						deliveryLetter.SentOn);

					await _stationLetterClient.PostStationLetterAsync(
						request,
						cancellationToken).ConfigureAwait(false);
				},
				cancellationToken)
			.ConfigureAwait(false);
	}
}
