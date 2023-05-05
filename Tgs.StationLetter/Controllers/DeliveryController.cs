using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.ApplicationServices;
using Tgs.StationLetterService.ViewModels;

namespace Tgs.StationLetterService.Controllers;

[Route("delivery-box")]
[ApiController]
public class DeliveryController : ControllerBase
{
	[HttpGet("{sender}", Name = "ListDeliveryLettersBySender")]
	public IAsyncEnumerable<DeliveryLetterViewModel> ListDeliveryLettersBySenderAsync(
		[FromServices] IQueryService<GetDeliveryLettersByUserQuery, DeliveryLetter> queryService,
		string sender)
	{
		var query = new GetDeliveryLettersByUserQuery(sender);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(letter => new DeliveryLetterViewModel
			{
				Id = letter.Id,
				Sender = letter.Sender,
				ReceiverType = letter.ReceiverType,
				Receiver = letter.Receiver,
				Subject = letter.Subject,
				Content = letter.Content,
				SentOn = letter.SentOn
			});
	}

	[HttpGet("{deliveryId:Guid}", Name = "GetDeliveryLetterById")]
	public async Task<DeliveryLetterViewModel?> GetDeliveryLetterByIdAsync(
		[FromServices] IGetService<GetDeliveryLetterByIdQuery, DeliveryLetter?> getLetterService,
		Guid deliveryId)
	{
		var query = new GetDeliveryLetterByIdQuery(deliveryId);

		var deliveryLetter = await getLetterService.GetAsync(
			query,
			HttpContext.RequestAborted).ConfigureAwait(false);

		return deliveryLetter is null
			? null
			: new DeliveryLetterViewModel
			{
				Id = deliveryLetter.Id,
				Sender = deliveryLetter.Sender,
				ReceiverType = deliveryLetter.ReceiverType,
				Receiver = deliveryLetter.Receiver,
				Subject = deliveryLetter.Subject,
				Content = deliveryLetter.Content,
				ComeFrom = deliveryLetter.ComeFrom,
				SentOn = deliveryLetter.SentOn
			};
	}
}
