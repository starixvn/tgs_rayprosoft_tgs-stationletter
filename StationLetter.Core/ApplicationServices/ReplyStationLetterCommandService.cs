using Tgs.ServiceUtility;

namespace Tgs.StationLetter.ApplicationServices;

internal class ReplyStationLetterCommandService : ICommandService<ReplyStationLetterCommand>
{
	private readonly IMailSlot _mailSlot;

	public ReplyStationLetterCommandService(IMailSlot mailSlot)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
	}

	public ValueTask ExecuteAsync(ReplyStationLetterCommand command, CancellationToken cancellationToken = default)
		=> _mailSlot.DeliveryAsync(new SendLetter(
			command.Sender,
			new[] { command.Receiver },
			null,
			command.Content,
			command.ReplyLetterId,
			DateTime.UtcNow), cancellationToken);
}
