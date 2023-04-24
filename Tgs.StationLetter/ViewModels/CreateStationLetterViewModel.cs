namespace Tgs.StationLetterService.ViewModels;

public class CreateStationLetterViewModel
{
	public required string Sender { get; set; }

	public required string[] Receivers { get; set; }

	public required string Subject { get; set; }

	public required string Content { get; set; }
}
