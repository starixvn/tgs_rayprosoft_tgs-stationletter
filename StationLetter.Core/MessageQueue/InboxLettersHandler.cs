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

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = SendLetterRequest.Parser.ParseFrom(data.Span);

		var letters = _mailSlot.GetWaitingSendLettersAsync(request.SendLetterId, cancellationToken);

		return _stationLetterPostman.InboxAsync(letters.ToEnumerable(), cancellationToken);
	}
}
