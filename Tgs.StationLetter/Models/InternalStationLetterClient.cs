using Tgs.ServiceUtility;
using Tgs.StationLetterService.ApplicationServices;
using Tgs.StationLetterService.Client;

namespace Tgs.StationLetterService.Models;

internal class InternalStationLetterClient : IStationLetterClient
{
	private readonly IServiceCoordinator _serviceCoordinator;

	public InternalStationLetterClient(IServiceCoordinator serviceCoordinator)
	{
		_serviceCoordinator = serviceCoordinator ?? throw new ArgumentNullException(nameof(serviceCoordinator));
	}

	public async ValueTask<Client.StationLetter?> GetStationLetterAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var query = new GetLetterByIdQuery(letterId);

		var letter = await _serviceCoordinator.GetAsync<GetLetterByIdQuery, Letter?>(
			query,
			cancellationToken).ConfigureAwait(false);

		return letter is null
			? null
			: new Client.StationLetter(
				letter.Id,
				letter.Sender,
				letter.Receiver,
				letter.Subject,
				letter.Quotation,
				letter.Content,
				letter.ComeFrom,
				letter.SentOn,
				letter.HaveRead,
				letter.Replied);
	}

	public IAsyncEnumerable<Client.StationLetter> ListStationLettersAsync(string receiver, CancellationToken cancellationToken = default)
	{
		var query = new GetLettersByUserQuery(receiver);

		return _serviceCoordinator.QueryAsync<GetLettersByUserQuery, Letter>(query, cancellationToken)
			.Select(letter => new Client.StationLetter(
				letter.Id,
				letter.Sender,
				letter.Receiver,
				letter.Subject,
				letter.Quotation,
				letter.Content,
				letter.ComeFrom,
				letter.SentOn,
				letter.HaveRead,
				letter.Replied));
	}

	public ValueTask MarkStationLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default)
	{
		var command = new SetLetterHaveReadCommand(letterId);

		return _serviceCoordinator.ExecuteAsync(command, cancellationToken);
	}

	public ValueTask ReplyStationLetterAsync(Guid letterId, ReplyStationLetterRequest request, CancellationToken cancellationToken = default)
	{
		var command = new ReplyStationLetterCommand(
			request.Sender,
			request.Receiver,
			letterId,
			request.Content);

		return _serviceCoordinator.ExecuteAsync(command, cancellationToken);
	}

	public ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default)
		=> throw new NotSupportedException();

	public ValueTask SendBrandAllPlayersStationLetterAsync(BrandAllPlayersStationLetterRequest request, CancellationToken cancellationToken = default)
		=> throw new NotSupportedException();

	public IAsyncEnumerable<Client.DeliveryLetter> ListDeliverLettersAsync(string sender, CancellationToken cancellationToken = default)
	{
		var query = new GetDeliveryLettersByUserQuery(sender);

		return _serviceCoordinator.QueryAsync<GetDeliveryLettersByUserQuery, DeliveryLetter>(query, cancellationToken)
			.Select(letter => new Client.DeliveryLetter(
				letter.Id,
				letter.Sender,
				letter.ReceiverType,
				letter.Receiver,
				letter.Subject,
				letter.Content,
				letter.ComeFrom,
				letter.SentOn));
	}

	public async ValueTask<Client.DeliveryLetter?> GetDeliveryLetterAsync(Guid deliveryId, CancellationToken cancellationToken = default)
	{
		var query = new GetDeliveryLetterByIdQuery(deliveryId);

		var deliveryLetter = await _serviceCoordinator.GetAsync<GetDeliveryLetterByIdQuery, DeliveryLetter?>(
			query,
			cancellationToken).ConfigureAwait(false);

		return deliveryLetter is null
			? null
			: new Client.DeliveryLetter(
				deliveryLetter.Id,
				deliveryLetter.Sender,
				deliveryLetter.ReceiverType,
				deliveryLetter.Receiver,
				deliveryLetter.Subject,
				deliveryLetter.Content,
				deliveryLetter.ComeFrom,
				deliveryLetter.SentOn);
	}

	public ValueTask PostStationLetterAsync(PostStationLetterRequest request, CancellationToken cancellationToken = default)
	{
		var command = new DeliveryStationLetterCommand(
			request.Sender,
			request.Receivers,
			request.Subject,
			request.Content,
			request.ComeFrom,
			request.SentOn);

		return _serviceCoordinator.ExecuteAsync(command, cancellationToken);
	}
}
