using ExpectedObjects;
using NSubstitute;
using Tgs.StationLetter;
using Tgs.StationLetter.ApplicationServices;

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
}
