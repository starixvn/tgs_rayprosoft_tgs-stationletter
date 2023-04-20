namespace Tgs.StationLetter;

public interface IMailSlot
{
	ValueTask DeliveryAsync(SendLetter sendLetter, CancellationToken cancellationToken = default);
}
