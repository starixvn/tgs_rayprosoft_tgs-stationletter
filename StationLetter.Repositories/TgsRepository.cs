using Microsoft.EntityFrameworkCore;
using Tgs.StationLetter.Repositories.Entities;

namespace Tgs.StationLetter.Repositories;

internal class TgsRepository : ITgsRepository
{
	private readonly TgsContext _dbContext;

	public TgsRepository(TgsContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public async ValueTask InsertStationLettersAsync(IEnumerable<Letter> letters, CancellationToken cancellationToken = default)
	{
		var letterArray = letters.ToArray();
		var allIds = letterArray.Select(l => l.Id).ToArray();

		var existsLetters = await _dbContext.StationLetterRows
			.Where(row => allIds.Contains(row.Id))
			.Select(row => row.Id)
			.ToArrayAsync(cancellationToken)
			.ToAsyncEnumerable()
			.SelectMany(arr => arr.ToAsyncEnumerable())
			.ToHashSetAsync(cancellationToken)
			.ConfigureAwait(false);

		var insertLetters = letterArray
			.Where(l => existsLetters.Contains(l.Id))
			.Select(l => new StationLetterRow
			{
				Id = l.Id,
				Sender = l.Sender,
				Receiver = l.Receiver,
				Subject = l.Subject,
				Quotation = l.Quotation,
				Content = l.Content,
				ComeFrom = l.ComeFrom,
				SentOn = l.SentOn.ToUniversalTime(),
				ReceivedOn = l.ReceivedOn.ToUniversalTime(),
				HaveRead = l.HaveRead,
				Replied = l.Replied
			});

		await _dbContext.StationLetterRows.AddRangeAsync(
			insertLetters,
			cancellationToken).ConfigureAwait(false);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}
}
