using System.Reflection;
using Tgs.StationLetterService;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStationLetter(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddScoped<IMailSlot, MailSlot>()
			.AddTransient<IStationLetterPostman, StationLetterPostman>();
}
