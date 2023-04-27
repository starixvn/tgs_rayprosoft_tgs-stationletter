using Microsoft.Extensions.DependencyInjection;

namespace Tgs.StationLetterService.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStationLetterClient(
		this IServiceCollection services,
		Action<IHttpClientBuilder> httpClientConfigure)
	{
		httpClientConfigure?.Invoke(
			services.AddHttpClient<IStationLetterClient, StationLetterClient>());

		return services;
	}
}
