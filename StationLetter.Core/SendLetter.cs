namespace Tgs.StationLetter;
public record SendLetter(
	string Sender,
	IEnumerable<string> Receivers,
	string? Subject,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn);
