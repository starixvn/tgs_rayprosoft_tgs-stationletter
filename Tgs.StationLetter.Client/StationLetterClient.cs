using System.Net.Http.Json;
using System.Text.Json;

namespace Tgs.StationLetterService.Client;

internal class StationLetterClient
{
	private static readonly JsonSerializerOptions _JsonSerializerOptions = new(JsonSerializerDefaults.Web);
	private readonly HttpClient _httpClient;

	public StationLetterClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default)
		=> await _httpClient.PutAsJsonAsync(
			"/station-letter",
			request,
			_JsonSerializerOptions,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.FirstAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask ReplyStationLetterAsync(
		Guid letterId,
		ReplyStationLetterRequest request,
		CancellationToken cancellationToken = default)
		=> await _httpClient.PutAsJsonAsync(
			$"/station-letter/{letterId}",
			request,
			_JsonSerializerOptions,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.FirstAsync(cancellationToken)
			.ConfigureAwait(false);

	public IAsyncEnumerable<StationLetter> ListStationLettersAsync(string receiver, CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync($"/station-letter/{receiver}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<IAsyncEnumerable<StationLetter>>(_JsonSerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(arr => arr is not null)
				.SelectMany(arr => arr!));
}
