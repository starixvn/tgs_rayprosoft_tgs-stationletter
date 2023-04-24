using ExpectedObjects;
using NSubstitute;
using Tgs.StationLetterService;
using Tgs.StationLetterService.ApplicationServices;

namespace StationLetter.Core.UnitTests;

public class SendStationLetterTests
{
	[Fact]
	public async Task 發送站內信()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();

		var sut = new SendStationLetterCommandService(fakeMailSlot);

		var command = new SendStationLetterCommand(
			"sender",
			new[] { "receiver" },
			"test",
			"aabbcc");

		var expected = new
		{
			command.Sender,
			command.Receivers,
			command.Subject,
			command.Content,
			ComeFrom = default(Guid?)
		};

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeMailSlot.Received(1)
			.DeliveryAsync(Arg.Is<SendLetter>(
				sendLetter => expected.ToExpectedObject().Matches(sendLetter)));
	}

	[Fact]
	public async Task 回信()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new ReplyStationLetterCommandService(fakeMailSlot, fakeTgsRepository);

		var command = new ReplyStationLetterCommand(
			"receiver",
			"sender",
			Guid.NewGuid(),
			"ddeeff");

		var expected = new
		{
			command.Sender,
			Receivers = new[] { command.Receiver },
			Subject = (string?)null,
			command.Content,
			ComeFrom = command.ReplyLetterId
		};

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeMailSlot.Received(1)
			.DeliveryAsync(Arg.Is<SendLetter>(
				sendLetter => expected.ToExpectedObject().Matches(sendLetter)));
		_ = fakeTgsRepository.Received(1)
			.SetLetterRepliedAsync(Arg.Is(command.ReplyLetterId));
	}

	[Fact]
	public async Task 取得使用者的站內信列表()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetLettersByUserQueryService(fakeTgsRepository);

		var query = new GetLettersByUserQuery("user");

		// Act
		_ = await sut.QueryAsync(query).ToArrayAsync();

		// Assert
		_ = fakeTgsRepository.Received(1)
			.QueryLettersByUserAsync(Arg.Is(query.User));
	}

	[Fact]
	public async Task 標記站內信已讀()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new SetLetterHaveReadCommandService(fakeTgsRepository);

		var command = new SetLetterHaveReadCommand(Guid.NewGuid());

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.SetLetterHaveReadAsync(Arg.Is(command.LetterId));
	}
}
