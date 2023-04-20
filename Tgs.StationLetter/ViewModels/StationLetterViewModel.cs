namespace Tgs.StationLetter.ViewModels;

public class StationLetterViewModel
{
	public required Guid Id { get; set; }

	public required string Subject { get; set; }

	public required string Content { get; set; }

	public required DateTime SentOn { get; set; }

	public required string Sender { get; set; }

	public required string Receiver { get; set; }

	public required bool HaveRead { get; set; }

	public required bool Replied { get; set; }
}
