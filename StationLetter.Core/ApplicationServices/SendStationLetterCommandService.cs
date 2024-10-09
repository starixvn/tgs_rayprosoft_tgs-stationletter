using Google.Protobuf;
using Tgs.ServiceUtility;
using Tgs.StationLetter.Protos;
using Valhalla.MessageQueue;

namespace Tgs.StationLetterService.ApplicationServices;

internal class SendStationLetterCommandService : ICommandService<SendStationLetterCommand>
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IMessageSender _messageSender;

	public SendStationLetterCommandService(ITgsRepository tgsRepository, IMessageSender messageSender)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public async ValueTask ExecuteAsync(SendStationLetterCommand command, CancellationToken cancellationToken = default)
	{
		var deliveryLetter = new DeliveryLetter(
			Guid.NewGuid(),
			command.Sender,
			command.ReceiverType,
			command.Receiver,
			command.Subject,
			command.Content,
			null,
			DateTime.UtcNow,
			command.Operator);

		await _tgsRepository.InsertDeliveryLetterAsync(
			deliveryLetter,
			cancellationToken).ConfigureAwait(false);

		var request = new DeliverLetterRequest
		{
			DeliverLetterId = deliveryLetter.Id
		};

		await _messageSender.PublishAsync(
			//$"tgs.station-letter.dispatch.{deliveryLetter.ReceiverType.ToString().ToLowerInvariant()}",
			$"tgs.station-letter.dispatch.{ReceiverType.Receivers.ToString().ToLowerInvariant()}",
			request.ToByteArray(),
			cancellationToken).ConfigureAwait(false);
	}
}
