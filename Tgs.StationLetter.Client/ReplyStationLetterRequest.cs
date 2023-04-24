namespace Tgs.StationLetter.Client;
public record ReplyStationLetterRequest(
	string Sender,
	string Receiver,
	string Content);
