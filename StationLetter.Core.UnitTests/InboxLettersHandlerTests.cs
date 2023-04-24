using Google.Protobuf;
using NSubstitute;
using Tgs.StationLetter;
using Tgs.StationLetter.MessageQueue;
using Tgs.StationLetter.Protos;

namespace StationLetter.Core.UnitTests;

public class InboxLettersHandlerTests
{
	[Fact]
	public async Task 將暫存信件投遞到目標信箱()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakePostman = Substitute.For<IStationLetterPostman>();

		var sut = new InboxLettersHandler(fakeMailSlot, fakePostman);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		_ = fakeMailSlot.GetWaitingSendLettersAsync(Arg.Is(request.SendLetterId.ToGuid()))
			.Returns(new[]
			{
				new Letter(
					Guid.NewGuid(),
					"sender",
					"receiver",
					"abc",
					null,
					"aabbcc",
					null,
					DateTime.Now,
					DateTime.Now,
					false,
					false)
			}.ToAsyncEnumerable());

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakePostman.Received(1)
			.InboxAsync(Arg.Any<IEnumerable<Letter>>());
		_ = fakeMailSlot.Received(1)
			.CompleteSendLetterAsync(Arg.Is(request.SendLetterId.ToGuid()));
	}
}
