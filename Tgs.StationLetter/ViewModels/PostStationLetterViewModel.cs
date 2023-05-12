namespace Tgs.StationLetterService.ViewModels;

public class PostStationLetterViewModel
{
	public required string Sender { get; set; }

	public required IEnumerable<string> Receivers { get; set; }

	public required string Subject { get; set; }

	public required string Content { get; set; }

	public Guid? ComeFrom { get; set; }

	public required DateTime SentOn { get; set; }
}
