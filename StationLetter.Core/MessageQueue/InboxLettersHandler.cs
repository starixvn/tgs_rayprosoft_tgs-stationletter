using Tgs.MessageQueue;
using Tgs.StationLetter.Protos;

namespace Tgs.StationLetter.MessageQueue;

internal class InboxLettersHandler : IMessageHandler
{
	private readonly IMailSlot _mailSlot;
	private readonly IStationLetterPostman _stationLetterPostman;

	public InboxLettersHandler(IMailSlot mailSlot, IStationLetterPostman stationLetterPostman)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
		_stationLetterPostman = stationLetterPostman ?? throw new ArgumentNullException(nameof(stationLetterPostman));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SendLetterRequest.Parser.ParseFrom(data.Span);

		try
		{
			var letters = _mailSlot.GetWaitingSendLettersAsync(request.SendLetterId, cancellationToken);

			await _stationLetterPostman.InboxAsync(
				letters.ToEnumerable(),
				cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			await _mailSlot.CompleteSendLetterAsync(
				request.SendLetterId,
				cancellationToken).ConfigureAwait(false);
		}
	}
}
