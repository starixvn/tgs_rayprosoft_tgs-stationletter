using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.ApplicationServices;
using Tgs.StationLetterService.ViewModels;
using Tgs.WebApi.Toolkits;

namespace Tgs.StationLetterService.Controllers;

[Route("station-letter")]
[ApiController]
public class StationLetterController : ControllerBase
{
	[HttpPut(Name = "SendStationLetter")]
	[SuccessStatusCode(HttpStatusCode.Accepted)]
	public ValueTask SendLetterAsync(
		[FromServices] ICommandService<SendStationLetterCommand> sendService,
		CreateStationLetterViewModel viewModel)
	{
		var command = new SendStationLetterCommand(
			viewModel.Sender,
			viewModel.ReceiverType,
			viewModel.Receiver,
			viewModel.Subject,
			viewModel.Content,
			viewModel.Operator);

		return sendService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPut("{letterId}", Name = "ReplyStationLetter")]
	[SuccessStatusCode(HttpStatusCode.Accepted)]
	public ValueTask ReplyLetterAsync(
		[FromServices] ICommandService<ReplyStationLetterCommand> sendService,
		Guid letterId,
		ReplyStationLetterViewModel viewModel)
	{
		var command = new ReplyStationLetterCommand(
			viewModel.Sender,
			viewModel.Receiver,
			letterId,
			viewModel.Content,
			viewModel.Operator);

		return sendService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpGet("{receiver}", Name = "ListStationLettersByReceiver")]
	public IAsyncEnumerable<StationLetterViewModel> ListLettersByReceiverAsync(
		[FromServices] IQueryService<GetLettersByUserQuery, Letter> queryService,
		string receiver)
	{
		var query = new GetLettersByUserQuery(receiver);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(letter => new StationLetterViewModel
			{
				Id = letter.Id,
				Sender = letter.Sender,
				Receiver = letter.Receiver,
				Subject = letter.Subject,
				Quotation = letter.Quotation,
				Content = letter.Content,
				SentOn = letter.SentOn,
				HaveRead = letter.HaveRead,
				Replied = letter.Replied
			});
	}

	[HttpGet("{letterId:Guid}", Name = "GetStationLetterById")]
	public async ValueTask<StationLetterViewModel?> GetLetterByIdAsync(
		[FromServices] IGetService<GetLetterByIdQuery, Letter?> getService,
		Guid letterId)
	{
		var query = new GetLetterByIdQuery(letterId);

		var letter = await getService.GetAsync(query, HttpContext.RequestAborted).ConfigureAwait(false);

		return letter is null
			? null
			: new StationLetterViewModel
			{
				Id = letter.Id,
				Sender = letter.Sender,
				Receiver = letter.Receiver,
				Subject = letter.Subject,
				Quotation = letter.Quotation,
				Content = letter.Content,
				SentOn = letter.SentOn,
				HaveRead = letter.HaveRead,
				Replied = letter.Replied
			};
	}

	[HttpPatch("{letterId}:read", Name = "MarkStationLetterHaveRead")]
	public ValueTask MarkLetterHaveReadAsync(
		[FromServices] ICommandService<SetLetterHaveReadCommand> patchService,
		Guid letterId)
	{
		var command = new SetLetterHaveReadCommand(letterId);

		return patchService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}
