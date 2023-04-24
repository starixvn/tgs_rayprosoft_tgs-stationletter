namespace Tgs.StationLetterService;
public record Letter(
	Guid Id,
	string Sender,
	string Receiver,
	string Subject,
	string? Quotation,
	string Content,
	Guid? ComeFrom,
	DateTime SentOn,
	DateTime ReceivedOn,
	bool HaveRead,
	bool Replied);
