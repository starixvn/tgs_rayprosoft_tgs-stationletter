using Google.Protobuf;
using Tgs.MessageQueue;
using Tgs.StationLetter.Protos;

namespace Tgs.StationLetter;

internal class MailSlot : IMailSlot
{
	private readonly IMailSlotRepository _mailSlotRepository;
	private readonly ITgsRepository _tgsRepository;
	private readonly IMessageSender _messageSender;

	public MailSlot(IMailSlotRepository mailSlotRepository, ITgsRepository tgsRepository, IMessageSender messageSender)
	{
		_mailSlotRepository = mailSlotRepository ?? throw new ArgumentNullException(nameof(mailSlotRepository));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	public async ValueTask DeliveryAsync(SendLetter sendLetter, CancellationToken cancellationToken = default)
	{
		var id = await _mailSlotRepository.SaveSendLetterAsync(
			sendLetter,
			cancellationToken).ConfigureAwait(false);

		var request = new SendLetterRequest
		{
			SendLetterId = id
		};

		await _messageSender.PublishAsync(
			"tgs.station-letter.delivery",
			request.ToByteArray(),
			cancellationToken).ConfigureAwait(false);
	}

	public ValueTask<SendLetter?> GetSendLetterAsync(Guid id, CancellationToken cancellationToken = default)
		=> _mailSlotRepository.GetSendLetterAsync(id, cancellationToken);

	public ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default)
		=> _tgsRepository.GetLetterAsync(letterId, cancellationToken);

	public ValueTask ScheduleLetterAsync(Guid id, Letter letter, CancellationToken cancellationToken = default)
		=> _mailSlotRepository.SaveLetterAsync(id, letter, cancellationToken);

	public IAsyncEnumerable<Letter> GetWaitingSendLettersAsync(Guid id, CancellationToken cancellationToken = default)
		=> _mailSlotRepository.GetLettersAsync(id, cancellationToken);

	public ValueTask CompleteSendLetterAsync(Guid id, CancellationToken cancellationToken = default)
		=> _mailSlotRepository.CleanUpSendLetterAsync(id, cancellationToken);
}
