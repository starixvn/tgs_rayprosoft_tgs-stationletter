using System.Text.Json;

namespace Tgs.StationLetterService.Client;
internal record SendStationLetterRequest(
	string Sender,
	ReceiverType ReceiverType,
	JsonDocument? Receiver,
	string Subject,
	string Content,
	string Operator);
