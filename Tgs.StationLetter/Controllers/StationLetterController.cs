using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.StationLetter.ApplicationServices;
using Tgs.StationLetter.ViewModels;
using Tgs.WebApi.Toolkits;

namespace Tgs.StationLetter.Controllers;

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
			viewModel.Receivers,
			viewModel.Subject,
			viewModel.Content);

		return sendService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPut("{letterId}", Name = "ReplyLetter")]
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
			viewModel.Content);

		return sendService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	//public IEnumerable<StationLetterViewModel> ListLettersByReceiver()
	//{
	//	return Array.Empty<StationLetterViewModel>();
	//}

	//public void MarkLetterHaveRead(Guid letterId)
	//{
	//}
}
