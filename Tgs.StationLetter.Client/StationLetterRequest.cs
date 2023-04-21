namespace Tgs.StationLetter.Client;
public record StationLetterRequest(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content);
