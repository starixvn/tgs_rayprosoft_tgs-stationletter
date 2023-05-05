using System.Text.Json;

namespace Tgs.StationLetterService.Client;
public record DeliveryLetter(
	Guid Id,
	string Sender,
	ReceiverType ReceiverType,
	JsonDocument? Receiver,
	string Subject,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn);
