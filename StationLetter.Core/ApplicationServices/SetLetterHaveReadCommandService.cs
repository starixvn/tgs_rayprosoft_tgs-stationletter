using Tgs.ServiceUtility;

namespace Tgs.StationLetterService.ApplicationServices;

internal class SetLetterHaveReadCommandService : ICommandService<SetLetterHaveReadCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public SetLetterHaveReadCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask ExecuteAsync(SetLetterHaveReadCommand command, CancellationToken cancellationToken = default)
		=> _tgsRepository.SetLetterHaveReadAsync(command.LetterId, cancellationToken);
}
