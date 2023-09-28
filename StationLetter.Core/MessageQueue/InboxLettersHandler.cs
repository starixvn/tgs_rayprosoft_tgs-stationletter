using System.Runtime.CompilerServices;
using Tgs.StationLetter.Protos;
using Valhalla.MessageQueue;

namespace Tgs.StationLetterService.MessageQueue;

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
			await foreach (var letters in GetSendLetterBatchAsync(
				request.SendLetterId,
				cancellationToken).ConfigureAwait(false))
			{
				await _stationLetterPostman.InboxAsync(
					letters,
					cancellationToken).ConfigureAwait(false);

				await _mailSlot.CompleteWaitingSendLetterBatchAsync(
					request.SendLetterId,
					letters.Length,
					cancellationToken).ConfigureAwait(false);
			}
		}
		finally
		{
			await _mailSlot.CompleteSendLetterAsync(
				request.SendLetterId,
				cancellationToken).ConfigureAwait(false);
		}
	}

	private async IAsyncEnumerable<Letter[]> GetSendLetterBatchAsync(
		Guid sendLetterId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		while (true)
		{
			var batch = await _mailSlot.GetWaitingSendLettersAsync(
				sendLetterId,
				cancellationToken).ToArrayAsync(cancellationToken).ConfigureAwait(false);

			if (batch.Length == 0)
				yield break;

			yield return batch;
		}
	}
}
