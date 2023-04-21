using System.Reflection;
using Tgs.StationLetter;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStationLetter(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<IMailSlot, MailSlot>()
			.AddTransient<IStationLetterPostman, StationLetterPostman>();
}
