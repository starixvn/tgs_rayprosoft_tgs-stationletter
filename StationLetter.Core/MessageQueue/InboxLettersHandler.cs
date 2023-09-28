using System.Runtime.CompilerServices;
using Tgs.Notification.Client;
using Tgs.StationLetter.Protos;
using Valhalla.MessageQueue;

namespace Tgs.StationLetterService.MessageQueue;

internal class InboxLettersHandler : IMessageHandler
{
	private readonly IMailSlot _mailSlot;
	private readonly IStationLetterPostman _stationLetterPostman;
	private readonly INotificationClient _notificationClient;

	public InboxLettersHandler(
		IMailSlot mailSlot,
		IStationLetterPostman stationLetterPostman,
		INotificationClient notificationClient)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
		_stationLetterPostman = stationLetterPostman ?? throw new ArgumentNullException(nameof(stationLetterPostman));
		_notificationClient = notificationClient ?? throw new ArgumentNullException(nameof(notificationClient));
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

				if (letters is not null)
					await Task.WhenAll(letters.Select(letter => _notificationClient
						.PushNotificationAsync(
							letter.Receiver,
							"LetterInBox",
							null,
							cancellationToken)
						.AsTask())
						.ToArray()).ConfigureAwait(false);
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
