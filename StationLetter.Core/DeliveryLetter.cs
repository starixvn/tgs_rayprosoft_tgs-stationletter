using System.Text.Json;

namespace Tgs.StationLetterService;
public record DeliveryLetter(
	Guid Id,
	string Sender,
	ReceiverType ReceiverType,
	JsonDocument Receiver,
	string Subject,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn);
