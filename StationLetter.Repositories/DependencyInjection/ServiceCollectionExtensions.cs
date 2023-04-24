using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tgs.StationLetterService;
using Tgs.StationLetterService.Repositories;
using Tgs.StationLetterService.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStationLetterRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbContextConfigure,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
			.AddDbContext<TgsContext>(dbContextConfigure)
			.AddTransient<ITgsRepository, TgsRepository>()
			.AddTransient<IMailSlotRepository>(sp => ActivatorUtilities.CreateInstance<MailSlotRepository>(
				sp,
				redisDatabaseResolver(sp)));
}
