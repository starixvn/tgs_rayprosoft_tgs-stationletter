using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.MessageQueue;
using Tgs.StationLetter;
using Tgs.StationLetter.MessageQueue;
using Tgs.StationLetter.Protos;

namespace StationLetter.Core.UnitTests;

public class SendLetterHandlerTests
{
	[Fact]
	public async Task 將發送站內信要求轉換成信件資料批次()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new SendLetterHandler(
			fakeMailSlot,
			fakeMessageSender,
			NullLogger<SendLetterHandler>.Instance);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		_ = fakeMailSlot.GetSendLetterAsync(Arg.Is(request.SendLetterId.ToGuid()))
			.Returns(new SendLetter(
				"sender",
				new[] { "receiver" },
				"test",
				"aabbcc",
				null,
				DateTime.Now));

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeMailSlot.Received(1)
			.ScheduleLetterAsync(Arg.Any<Letter>());
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.station-letter.inbox"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 處理發送站內信要求如果找不到要求的資料不會往下處理()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new SendLetterHandler(
			fakeMailSlot,
			fakeMessageSender,
			NullLogger<SendLetterHandler>.Instance);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeMessageSender.DidNotReceive()
			.PublishAsync(Arg.Is("tgs.station-letter.inbox"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 處理回信要求如果找不到上一封信不會往下處理()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new SendLetterHandler(
			fakeMailSlot,
			fakeMessageSender,
			NullLogger<SendLetterHandler>.Instance);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		_ = fakeMailSlot.GetSendLetterAsync(Arg.Is(request.SendLetterId.ToGuid()))
			.Returns(new SendLetter(
				"sender",
				new[] { "receiver" },
				null,
				"aabbcc",
				Guid.NewGuid(),
				DateTime.Now));

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeMessageSender.DidNotReceive()
			.PublishAsync(Arg.Is("tgs.station-letter.inbox"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 處理回信要求()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new SendLetterHandler(
			fakeMailSlot,
			fakeMessageSender,
			NullLogger<SendLetterHandler>.Instance);

		var request = new SendLetterRequest
		{
			SendLetterId = Guid.NewGuid()
		};

		var sendLetter = new SendLetter(
			"sender",
			new[] { "receiver" },
			null,
			"aabbcc",
			Guid.NewGuid(),
			DateTime.Now);

		_ = fakeMailSlot.GetSendLetterAsync(Arg.Is(request.SendLetterId.ToGuid()))
			.Returns(sendLetter);

		_ = fakeMailSlot.GetLetterAsync(Arg.Is(sendLetter.ComeFrom!.Value))
			.Returns(new Letter(
				sendLetter.ComeFrom!.Value,
				"receiver",
				"sender",
				"subject",
				null,
				"abcde",
				null,
				DateTime.UtcNow,
				DateTime.UtcNow,
				true,
				false));

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeMailSlot.Received(1)
			.ScheduleLetterAsync(Arg.Any<Letter>());
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.station-letter.inbox"), Arg.Any<ReadOnlyMemory<byte>>());
	}
}
