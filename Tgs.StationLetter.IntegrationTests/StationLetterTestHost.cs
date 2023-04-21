using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Tgs.StationLetter.IntegrationTests;

internal static class StationLetterTestHost
{
	public static WebApplicationFactory<Program> Factory => new WebApplicationFactory<Program>()
		.WithWebHostBuilder(hostBildier => hostBildier
			.ConfigureAppConfiguration(configBuilder => configBuilder.Sources.Clear())
			.ConfigureLogging(logging => logging.ClearProviders())
			.ConfigureServices(services => services
				.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
					sp => new ConfigureNamedOptions<OtlpExporterOptions>(
						Options.DefaultName,
						exporterOptions => exporterOptions.TimeoutMilliseconds = 0))
				.AddFakeNatsMessageQueue()));
}
