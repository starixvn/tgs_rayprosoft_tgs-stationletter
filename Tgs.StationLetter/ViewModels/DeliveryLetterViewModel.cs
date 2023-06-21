using System.Text.Json;

namespace Tgs.StationLetterService.ViewModels;

public class DeliveryLetterViewModel
{
	public required Guid Id { get; set; }

	public required string Sender { get; set; }

	public required ReceiverType ReceiverType { get; set; }

	public JsonDocument? Receiver { get; set; }

	public required string Subject { get; set; }

	public required string Content { get; set; }

	public Guid? ComeFrom { get; set; }

	public DateTime SentOn { get; set; }

	public required string Operator { get; set; }
}
