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
			.Where(l => !existsLetters.Contains(l.Id))
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

	public async ValueTask<Letter?> GetLetterAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var letterRow = await _dbContext.StationLetterRows.FindAsync(
			new object?[] { letterId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return letterRow is null
			? null
			: new Letter(
				letterRow.Id,
				letterRow.Sender,
				letterRow.Receiver,
				letterRow.Subject,
				letterRow.Quotation,
				letterRow.Content,
				letterRow.ComeFrom,
				letterRow.SentOn,
				letterRow.ReceivedOn,
				letterRow.HaveRead,
				letterRow.Replied);
	}

	public async ValueTask SetLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var letterRow = await _dbContext.StationLetterRows.FindAsync(
			new object?[] { letterId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		if (letterRow is null || letterRow.HaveRead is true)
			return;

		letterRow.HaveRead = true;

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SetLetterRepliedAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var letterRow = await _dbContext.StationLetterRows.FindAsync(
			new object?[] { letterId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		if (letterRow is null || letterRow.Replied is true)
			return;

		letterRow.Replied = true;

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}
}
