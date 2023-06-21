namespace Tgs.StationLetterService.ApplicationServices;
public record ReplyStationLetterCommand(
	string Sender,
	string Receiver,
	Guid ReplyLetterId,
	string Content,
	string Operator);
