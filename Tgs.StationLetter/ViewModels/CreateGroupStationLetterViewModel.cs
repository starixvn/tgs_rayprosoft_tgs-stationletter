using System.Text.Json;

namespace Tgs.StationLetterService.ViewModels;

public class CreateGroupStationLetterViewModel
{
	public required string Sender { get; set; }

	public required ReceiverType ReceiverType { get; set; }

	public JsonDocument? Receiver { get; set; }

	public required string Subject { get; set; }

	public required string Content { get; set; }
}
