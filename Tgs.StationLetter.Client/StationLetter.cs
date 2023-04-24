namespace Tgs.StationLetterService.Client;
public record StationLetter(
	Guid Id,
	string Sender,
	string Receiver,
	string Subject,
	string? Quotation,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn,
	bool HaveRead,
	bool Replied);
