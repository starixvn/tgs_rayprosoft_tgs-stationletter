namespace Tgs.StationLetterService.Client;
public record BrandAllPlayersStationLetterRequest(
	string BrandCode,
	IEnumerable<string> Receivers,
	string Subject,
	string Content,
	string Operator);
