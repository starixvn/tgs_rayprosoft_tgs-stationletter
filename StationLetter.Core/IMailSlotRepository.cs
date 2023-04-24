namespace Tgs.StationLetter;

public interface IMailSlotRepository
{
	ValueTask<Guid> SaveSendLetterAsync(SendLetter sendLetter, CancellationToken cancellationToken = default);

	ValueTask<SendLetter?> GetSendLetterAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask SaveLetterAsync(Guid id, Letter letter, CancellationToken cancellationToken = default);

	IAsyncEnumerable<Letter> GetLettersAsync(Guid sendLetterId, CancellationToken cancellationToken = default);

	ValueTask CleanUpSendLetterAsync(Guid sendLetterId, CancellationToken cancellationToken = default);
}
