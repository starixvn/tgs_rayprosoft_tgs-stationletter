using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.StationLetter.ApplicationServices;
using Tgs.StationLetter.ViewModels;

namespace Tgs.StationLetter.Controllers;

[Route("station-letter")]
[ApiController]
public class StationLetterController : ControllerBase
{
	[HttpPut(Name = "SendStationLetter")]
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

	public void ReplyLetter(
		Guid letterId,
		ReplyStationLetterViewModel viewModel)
	{
	}

	public IEnumerable<StationLetterViewModel> ListLettersByReceiver()
	{
		return Array.Empty<StationLetterViewModel>();
	}

	public void MarkLetterHaveRead(Guid letterId)
	{
	}
}
