namespace Tgs.StationLetterService.Client;
public record PostStationLetterRequest(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn);
