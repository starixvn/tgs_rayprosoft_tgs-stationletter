using System.Text.Json;
using Google.Protobuf;
using NSubstitute;
using Tgs.Notification.Client;
using Tgs.StationLetter.Protos;
using Tgs.StationLetterService;
using Tgs.StationLetterService.MessageQueue;

namespace StationLetter.Core.UnitTests;

public class InboxLettersHandlerTests
{
	[Fact]
	public async Task 將暫存信件投遞到目標信箱()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakePostman = Substitute.For<IStationLetterPostman>();
		var fakeNotificationClient = Substitute.For<INotificationClient>();

		var sut = new InboxLettersHandler(
			fakeMailSlot,
			fakePostman,
			fakeNotificationClient);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		_ = fakeMailSlot.GetWaitingSendLettersAsync(Arg.Is(request.SendLetterId.ToGuid()))
			.Returns(
				new[]
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
				}.ToAsyncEnumerable(),
				Array.Empty<Letter>().ToAsyncEnumerable());

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakePostman.Received(1)
			.InboxAsync(Arg.Any<IEnumerable<Letter>>());
		_ = fakeMailSlot.Received(1)
			.CompleteSendLetterAsync(Arg.Is(request.SendLetterId.ToGuid()));
		_ = fakeMailSlot.Received(1)
			.CompleteWaitingSendLetterBatchAsync(Arg.Is(request.SendLetterId.ToGuid()), Arg.Is(1));
		_ = fakeNotificationClient.Received(1)
			.PushNotificationAsync(
				Arg.Is("receiver"),
				Arg.Is("LetterInBox"),
				Arg.Is(default(JsonDocument)));
	}
}
