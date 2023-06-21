using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tgs.StationLetterService.Repositories.Entities;

namespace Tgs.StationLetterService.Repositories;

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

	public IAsyncEnumerable<Letter> QueryLettersByUserAsync(string user, CancellationToken cancellationToken = default)
	{
		var queryDate = DateTime.UtcNow.AddDays(-90D);

		return _dbContext.StationLetterRows
			.Where(row => row.Receiver == user
				&& row.ReceivedOn >= queryDate)
			.OrderByDescending(row => row.SentOn)
			.Select(row => new Letter(
				row.Id,
				row.Sender,
				row.Receiver,
				row.Subject,
				row.Quotation,
				row.Content,
				row.ComeFrom,
				row.SentOn,
				row.ReceivedOn,
				row.HaveRead,
				row.Replied))
			.ToAsyncEnumerable()
			.Do(_ => cancellationToken.ThrowIfCancellationRequested());
	}

	public async ValueTask InsertDeliveryLetterAsync(DeliveryLetter deliveryLetter, CancellationToken cancellationToken = default)
	{
		_ = await _dbContext.StationLetterDeliveries
			.AddAsync(new StationLetterDelivery
			{
				Id = deliveryLetter.Id,
				Sender = deliveryLetter.Sender,
				ReceiverType = deliveryLetter.ReceiverType.ToString(),
				Receiver = JsonSerializer.Serialize(deliveryLetter.Receiver, StationLetterUtility.SerializerOptions),
				Subject = deliveryLetter.Subject,
				Content = deliveryLetter.Content,
				ComeFrom = deliveryLetter.ComeFrom,
				SentOn = deliveryLetter.SentOn.ToUniversalTime(),
				Operator = deliveryLetter.Operator
			}, cancellationToken).ConfigureAwait(false);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<DeliveryLetter?> GetDeliveryLetterAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var deliveryRow = await _dbContext.StationLetterDeliveries.FindAsync(
			new object?[] { letterId },
			cancellationToken).ConfigureAwait(false);

		return deliveryRow is null
			? null
			: new DeliveryLetter(
				deliveryRow.Id,
				deliveryRow.Sender,
				Enum.Parse<ReceiverType>(deliveryRow.ReceiverType),
				string.IsNullOrEmpty(deliveryRow.Receiver)
					? null
					: JsonDocument.Parse(deliveryRow.Receiver),
				deliveryRow.Subject,
				deliveryRow.Content,
				deliveryRow.ComeFrom,
				deliveryRow.SentOn,
				deliveryRow.Operator);
	}

	public IAsyncEnumerable<DeliveryLetter> QueryDeliveryLettersByUserAsync(string user, CancellationToken cancellationToken = default)
	{
		var queryDate = DateTime.UtcNow.AddDays(-90D);

		return _dbContext.StationLetterDeliveries
			.Where(row => row.Sender == user
				&& row.SentOn >= queryDate)
			.OrderByDescending(row => row.SentOn)
			.ToAsyncEnumerable()
			.Select(row => new DeliveryLetter(
				row.Id,
				row.Sender,
				Enum.Parse<ReceiverType>(row.ReceiverType),
				string.IsNullOrEmpty(row.Receiver)
					? null
					: JsonDocument.Parse(row.Receiver, default),
				row.Subject,
				row.Content,
				row.ComeFrom,
				row.SentOn,
				row.Operator))
			.Do(_ => cancellationToken.ThrowIfCancellationRequested());
	}
}
