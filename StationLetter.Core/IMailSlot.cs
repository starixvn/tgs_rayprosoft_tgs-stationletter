namespace Tgs.StationLetter;

public interface IMailSlot
{
	ValueTask DeliveryAsync(SendLetter sendLetter, CancellationToken cancellationToken = default);

	ValueTask<SendLetter?> GetSendLetterAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default);

	ValueTask ScheduleLetterAsync(Letter letter, CancellationToken cancellationToken = default);

	IAsyncEnumerable<Letter> GetWaitingSendLettersAsync(Guid id, CancellationToken cancellationToken = default);
}
