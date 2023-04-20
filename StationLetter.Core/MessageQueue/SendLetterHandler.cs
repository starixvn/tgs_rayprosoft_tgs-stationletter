using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.StationLetter.Protos;

namespace Tgs.StationLetter.MessageQueue;

internal class SendLetterHandler : IMessageHandler
{
	private readonly IStationLetterPostman _stationLetterPostman;
	private readonly ILogger<SendLetterHandler> _logger;

	public SendLetterHandler(IStationLetterPostman stationLetterPostman, ILogger<SendLetterHandler> logger)
	{
		_stationLetterPostman = stationLetterPostman ?? throw new ArgumentNullException(nameof(stationLetterPostman));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SendLetterRequest.Parser.ParseFrom(data.Span);

		var letterTemplate = new Letter(
			Guid.Empty,
			request.Sender,
			string.Empty,
			request.Subject,
			null,
			request.Content,
			request.ComeFrom,
			request.SentOn.ToDateTime(),
			request.SentOn.ToDateTime(),
			false,
			false);

		if (letterTemplate.ComeFrom.HasValue)
		{
			var comeFromLetter = await _stationLetterPostman.GetLetterAsync(
				letterTemplate.ComeFrom.Value,
				cancellationToken).ConfigureAwait(false);

			if (comeFromLetter is null)
			{
				_logger.LogError("Has come from letter, but not found.");
				return;
			}

			var quotation = comeFromLetter.Quotation;
			if (!string.IsNullOrEmpty(comeFromLetter.Quotation))
				quotation += "\n-------------------\n";
			quotation += comeFromLetter.Content;

			letterTemplate = letterTemplate with
			{
				Subject = $"Re:{comeFromLetter.Subject}",
				Quotation = quotation,
			};
		}

		foreach (var receiver in request.Receivers)
		{
			var letter = letterTemplate with
			{
				Id = Guid.NewGuid(),
				Receiver = receiver
			};

			await _stationLetterPostman.InboxAsync(
				letter,
				cancellationToken).ConfigureAwait(false);
		}
	}
}
