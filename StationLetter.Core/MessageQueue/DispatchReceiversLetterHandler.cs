using System.Text.Json;
using Tgs.StationLetter.Protos;
using Valhalla.MessageQueue;

namespace Tgs.StationLetterService.MessageQueue;

public class DispatchReceiversLetterHandler : IMessageHandler
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IMailSlot _mailSlot;

	public DispatchReceiversLetterHandler(ITgsRepository tgsRepository, IMailSlot mailSlot)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = DeliverLetterRequest.Parser.ParseFrom(data.Span);

		var deliveryLetter = await _tgsRepository.GetDeliveryLetterAsync(
			request.DeliverLetterId.ToGuid(),
			cancellationToken).ConfigureAwait(false);

		if (deliveryLetter is null)
			return;

		var receivers = deliveryLetter.Receiver?.Deserialize<string[]>(StationLetterUtility.SerializerOptions);
		if (receivers is null || receivers.Length == 0)
			return;

		await _mailSlot.DeliveryAsync(new SendLetter(
			deliveryLetter.Sender,
			receivers,
			deliveryLetter.Subject,
			deliveryLetter.Content,
			deliveryLetter.ComeFrom,
			deliveryLetter.SentOn), cancellationToken).ConfigureAwait(false);
	}
}
