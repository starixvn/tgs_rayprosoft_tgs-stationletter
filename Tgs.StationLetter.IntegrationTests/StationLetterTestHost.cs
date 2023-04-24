using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using Tgs.StationLetter.Repositories.Entities;

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
				.Replace(ServiceDescriptor.Singleton(sp =>
				{
					var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
					_ = dbContextOptionsBuilder
						.UseInMemoryDatabase(Guid.NewGuid().ToString())
						.ConfigureWarnings(x => x.Ignore(
							InMemoryEventId.TransactionIgnoredWarning));

					return new TgsContext(dbContextOptionsBuilder.Options);
				}))
				.AddFakeNatsMessageQueue()));
}
