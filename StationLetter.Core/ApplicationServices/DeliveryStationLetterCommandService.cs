using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class DeliveryStationLetterCommandService : ICommandService<DeliveryStationLetterCommand>
{
	private readonly IMailSlot _mailSlot;

	public DeliveryStationLetterCommandService(IMailSlot mailSlot)
	{
		_mailSlot = mailSlot ?? throw new ArgumentNullException(nameof(mailSlot));
	}

	public ValueTask ExecuteAsync(DeliveryStationLetterCommand command, CancellationToken cancellationToken = default)
		=> _mailSlot.DeliveryAsync(new SendLetter(
			command.Sender,
			command.Receivers,
			command.Subject,
			command.Content,
			command.ComeFrom,
			command.SentOn), cancellationToken);
}
