namespace Tgs.StationLetterService.Client;
public record BrandAllPlayersStationLetterRequest(
	string BrandCode,
	string Subject,
	string Content,
	string Operator);
