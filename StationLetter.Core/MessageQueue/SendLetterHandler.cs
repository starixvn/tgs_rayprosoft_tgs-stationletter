using Microsoft.Extensions.Logging;
using Tgs.StationLetter.Protos;
using Valhalla.MessageQueue;

namespace Tgs.StationLetterService.MessageQueue;

internal class SendLetterHandler : IMessageHandler
{
	private readonly IMailSlot _mailSlot;
	private readonly IMessageSender _messageSender;
	private readonly ILogger<SendLetterHandler> _logger;

	public SendLetterHandler(IMailSlot mailSlot, IMessageSender messageSender, ILogger<SendLetterHandler> logger)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SendLetterRequest.Parser.ParseFrom(data.Span);

		var sendLetter = await _mailSlot.GetSendLetterAsync(
			request.SendLetterId,
			cancellationToken).ConfigureAwait(false);

		if (sendLetter is null)
		{
			_logger.LogError("Can't found send letter request data.");
			return;
		}

		var letterTemplate = new Letter(
			Guid.Empty,
			sendLetter.Sender,
			string.Empty,
			sendLetter.Subject ?? string.Empty,
			null,
			sendLetter.Content,
			sendLetter.ComeFrom,
			sendLetter.SentOn,
			sendLetter.SentOn,
			false,
			false);

		if (letterTemplate.ComeFrom.HasValue)
		{
			var comeFromLetter = await _mailSlot.GetLetterAsync(
				letterTemplate.ComeFrom.Value,
				cancellationToken).ConfigureAwait(false);

			if (comeFromLetter is null)
			{
				_logger.LogError("Has come from letter, but not found.");
				return;
			}

			var quotation = comeFromLetter.Quotation;
			if (!string.IsNullOrEmpty(comeFromLetter.Quotation))
				quotation += "\n--------------------------\n";
			quotation += comeFromLetter.Content;

			letterTemplate = letterTemplate with
			{
				Subject = $"Re:{comeFromLetter.Subject}",
				Quotation = quotation,
			};
		}

		foreach (var receiver in sendLetter.Receivers)
		{
			var letter = letterTemplate with
			{
				Id = Guid.NewGuid(),
				Receiver = receiver
			};

			await _mailSlot.ScheduleLetterAsync(
				request.SendLetterId,
				letter,
				cancellationToken).ConfigureAwait(false);
		}

		await _messageSender.PublishAsync(
			"tgs.station-letter.inbox",
			data,
			cancellationToken).ConfigureAwait(false);
	}
}
