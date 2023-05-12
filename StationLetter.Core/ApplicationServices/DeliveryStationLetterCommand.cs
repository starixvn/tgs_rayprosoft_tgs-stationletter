namespace Tgs.StationLetterService.ApplicationServices;
public record DeliveryStationLetterCommand(
	string Sender,
	IEnumerable<string> Receivers,
	string Subject,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn);
