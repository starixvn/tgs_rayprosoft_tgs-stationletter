using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.StationLetterService.Client;

internal class StationLetterClient : IStationLetterClient
{
	private static readonly JsonSerializerOptions _JsonSerializerOptions = new(JsonSerializerDefaults.Web);

	private readonly HttpClient _httpClient;

	static StationLetterClient()
	{
		_JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	}

	public StationLetterClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async ValueTask SendStationLetterAsync(StationLetterRequest request, CancellationToken cancellationToken = default)
		=> await _httpClient.PutAsJsonAsync(
			"/station-letter",
			new SendStationLetterRequest(
				request.Sender,
				ReceiverType.Receivers,
				JsonSerializer.SerializeToDocument(request.Receivers, StationLetterUtility.SerializerOptions),
				request.Subject,
				request.Content),
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

	public async ValueTask MarkStationLetterHaveReadAsync(Guid letterId, CancellationToken cancellationToken = default)
		=> await _httpClient.PatchAsync($"/station-letter/{letterId}:read", null, cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.FirstAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask<StationLetter?> GetStationLetterAsync(Guid letterId, CancellationToken cancellationToken = default)
		=> await _httpClient.GetFromJsonAsync<StationLetter>(
			$"/station-letter/{letterId}",
			_JsonSerializerOptions,
			cancellationToken).ConfigureAwait(false);

	public IAsyncEnumerable<DeliveryLetter> ListDeliverLettersAsync(string sender, CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync($"/delivery-box/{sender}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<IAsyncEnumerable<DeliveryLetter>>(_JsonSerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(arr => arr is not null)
				.SelectMany(arr => arr!));

	public async ValueTask<DeliveryLetter?> GetDeliveryLetterAsync(Guid deliveryId, CancellationToken cancellationToken = default)
		=> await _httpClient.GetFromJsonAsync<DeliveryLetter>(
			$"/delivery-box/{deliveryId}",
			_JsonSerializerOptions,
			cancellationToken).ConfigureAwait(false);
}
