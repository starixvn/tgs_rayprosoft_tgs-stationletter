using System.Net.Http.Json;

namespace Tgs.StationLetter.Client;

internal class StationLetterClient
{
	private readonly HttpClient _httpClient;

	public StationLetterClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default)
		=> await _httpClient.PutAsJsonAsync("/station-letter", request, cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.FirstAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);
}
