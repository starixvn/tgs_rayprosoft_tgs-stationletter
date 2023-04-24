using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class ReplyStationLetterCommandService : ICommandService<ReplyStationLetterCommand>
{
	private readonly IMailSlot _mailSlot;
	private readonly ITgsRepository _tgsRepository;

	public ReplyStationLetterCommandService(IMailSlot mailSlot, ITgsRepository tgsRepository)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(ReplyStationLetterCommand command, CancellationToken cancellationToken = default)
	{
		await _mailSlot.DeliveryAsync(new SendLetter(
			command.Sender,
			new[] { command.Receiver },
			null,
			command.Content,
			command.ReplyLetterId,
			DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

		await _tgsRepository.SetLetterRepliedAsync(
			command.ReplyLetterId,
			cancellationToken).ConfigureAwait(false);
	}
}
