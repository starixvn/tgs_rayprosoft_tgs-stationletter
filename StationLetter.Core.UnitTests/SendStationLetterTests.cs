using System.Text.Json;
using ExpectedObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.StationLetterService;
using Tgs.StationLetterService.ApplicationServices;
using Valhalla.MessageQueue;

namespace StationLetter.Core.UnitTests;

public class SendStationLetterTests
{
	[Fact]
	public async Task 發送站內信()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new SendStationLetterCommandService(fakeTgsRepository, fakeMessageSender);

		var command = new SendStationLetterCommand(
			"sender",
			ReceiverType.Receivers,
			JsonSerializer.SerializeToDocument(new[] { "receiver" }, StationLetterUtility.SerializerOptions),
			"test",
			"aabbcc",
			"operator");

		var expected = new
		{
			command.Sender,
			ReceiverType = ReceiverType.Receivers,
			command.Subject,
			command.Content,
			ComeFrom = default(Guid?)
		};

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.InsertDeliveryLetterAsync(Arg.Is<DeliveryLetter>(
				delivery => expected.ToExpectedObject().Matches(delivery)));
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.station-letter.dispatch.receivers"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 回信()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new ReplyStationLetterCommandService(
			fakeTgsRepository,
			fakeMessageSender,
			NullLogger<ReplyStationLetterCommandService>.Instance);

		var command = new ReplyStationLetterCommand(
			"receiver",
			"sender",
			Guid.NewGuid(),
			"ddeeff",
			"operator");

		_ = fakeTgsRepository.GetLetterAsync(Arg.Is(command.ReplyLetterId))
			.Returns(new Letter(
				command.ReplyLetterId,
				command.Receiver,
				command.Sender,
				"ttt",
				null,
				"ccc",
				null,
				DateTime.UtcNow,
				DateTime.UtcNow,
				false,
				false));

		var expected = new
		{
			command.Sender,
			ReceiverType = ReceiverType.Receivers,
			Subject = "Re:ttt",
			command.Content,
			ComeFrom = command.ReplyLetterId,
			command.Operator
		};

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.InsertDeliveryLetterAsync(Arg.Is<DeliveryLetter>(
				delivery => expected.ToExpectedObject().Matches(delivery)));
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.station-letter.dispatch.receivers"), Arg.Any<ReadOnlyMemory<byte>>());
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

	[Fact]
	public async Task 依據站內信Id取得資料()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new GetLetterByIdQueryService(fakeTgsRepository);

		var query = new GetLetterByIdQuery(Guid.NewGuid());

		// Act
		_ = await sut.GetAsync(query);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.GetLetterAsync(Arg.Is(query.LetterId));
	}
}
