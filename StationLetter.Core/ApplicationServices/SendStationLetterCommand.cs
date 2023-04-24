namespace Tgs.StationLetterService.ApplicationServices;
public record SendStationLetterCommand(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content);
