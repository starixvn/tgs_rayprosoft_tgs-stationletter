namespace Tgs.StationLetter.ApplicationServices;
public record ReplyStationLetterCommand(
	string Sender,
	string Receiver,
	Guid ReplyLetterId,
	string Content);
