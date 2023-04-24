using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class SendStationLetterCommandService : ICommandService<SendStationLetterCommand>
{
	private readonly IMailSlot _mailSlot;

	public SendStationLetterCommandService(IMailSlot mailSlot)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
	}

	public ValueTask ExecuteAsync(SendStationLetterCommand command, CancellationToken cancellationToken = default)
		=> _mailSlot.DeliveryAsync(new SendLetter(
			command.Sender,
			command.Receivers,
			command.Subject,
			command.Content,
			null,
			DateTime.UtcNow), cancellationToken);
}
