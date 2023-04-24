namespace Tgs.StationLetterService.Client;
public record StationLetterRequest(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content);
