using NSubstitute;
using Tgs.StationLetterService;
using Tgs.StationLetterService.ApplicationServices;

namespace StationLetter.Core.UnitTests;

public class DeliveryLetterTests
{
	[Fact]
	public async Task 取得使用者的寄件備份列表()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetDeliveryLettersByUserQueryService(fakeTgsRepository);

		var query = new GetDeliveryLettersByUserQuery("test1");

		// Act
		_ = await sut.QueryAsync(query).ToArrayAsync();

		// Assert
		_ = fakeTgsRepository.Received(1)
			.QueryDeliveryLettersByUserAsync(Arg.Is(query.User));
	}

	[Fact]
	public async Task 取得寄件備份資料()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetDeliveryLetterByIdQueryService(fakeTgsRepository);

		var query = new GetDeliveryLetterByIdQuery(Guid.NewGuid());

		// Act
		_ = await sut.GetAsync(query);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.GetDeliveryLetterAsync(Arg.Is(query.DeliveryId));
	}

	[Fact]
	public async Task 傳送信件()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();

		var sut = new DeliveryStationLetterCommandService(fakeMailSlot);

		var command = new DeliveryStationLetterCommand(
			"GPI",
			new[] { "test1" },
			"ttt",
			"cccc",
			null,
			DateTime.UtcNow);

		var sendLetter = new SendLetter(
			command.Sender,
			command.Receivers,
			command.Subject,
			command.Content,
			command.ComeFrom,
			command.SentOn);

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeMailSlot.Received(1)
			.DeliveryAsync(Arg.Is(sendLetter));
	}
}
