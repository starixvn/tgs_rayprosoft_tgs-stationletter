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
}
