namespace Tgs.StationLetter;

public interface IMailSlotRepository
{
	ValueTask<Guid> SaveSendLetterAsync(SendLetter sendLetter, CancellationToken cancellationToken = default);

	ValueTask<SendLetter?> GetSendLetterAsync(Guid id, CancellationToken cancellationToken = default);

	ValueTask SaveLetterAsync(Guid id, Letter letter, CancellationToken cancellationToken = default);
}
