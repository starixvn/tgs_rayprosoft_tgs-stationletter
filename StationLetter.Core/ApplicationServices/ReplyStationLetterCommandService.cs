using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using Tgs.StationLetter.Protos;

namespace Tgs.StationLetterService.ApplicationServices;

internal class ReplyStationLetterCommandService : ICommandService<ReplyStationLetterCommand>
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IMessageSender _messageSender;
	private readonly ILogger<ReplyStationLetterCommandService> _logger;

	public ReplyStationLetterCommandService(
		ITgsRepository tgsRepository,
		IMessageSender messageSender,
		ILogger<ReplyStationLetterCommandService> logger)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(ReplyStationLetterCommand command, CancellationToken cancellationToken = default)
	{
		var letter = await _tgsRepository.GetLetterAsync(
			command.ReplyLetterId,
			cancellationToken).ConfigureAwait(false);

		if (letter is null)
		{
			_logger.LogError("Has come from letter, but not found.");
			return;
		}

		var deliveryLetter = new DeliveryLetter(
			Guid.NewGuid(),
			command.Sender,
			ReceiverType.Receivers,
			JsonSerializer.SerializeToDocument(new[] { command.Receiver }, StationLetterUtility.SerializerOptions),
			$"Re:{letter.Subject}",
			command.Content,
			command.ReplyLetterId,
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
			$"tgs.station-letter.dispatch.{deliveryLetter.ReceiverType.ToString().ToLowerInvariant()}",
			request.ToByteArray(),
			cancellationToken).ConfigureAwait(false);

		await _tgsRepository.SetLetterRepliedAsync(
			command.ReplyLetterId,
			cancellationToken).ConfigureAwait(false);
	}
}
