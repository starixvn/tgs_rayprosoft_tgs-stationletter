namespace Tgs.StationLetterService.Client;
public record ReplyStationLetterRequest(
	string Sender,
	string Receiver,
	string Content);
