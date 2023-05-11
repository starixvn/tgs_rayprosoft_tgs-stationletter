using System.Text.Json;

namespace Tgs.StationLetterService.ApplicationServices;
public record SendStationLetterCommand(
	string Sender,
	ReceiverType ReceiverType,
	JsonDocument? Receiver,
	string Subject,
	string Content);
