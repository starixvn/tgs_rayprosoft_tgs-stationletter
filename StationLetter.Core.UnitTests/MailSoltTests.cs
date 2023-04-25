using NSubstitute;
using Tgs.MessageQueue;
using Tgs.StationLetterService;

namespace StationLetter.Core.UnitTests;

public class MailSoltTests
{
	[Fact]
	public async Task 排程發送要求()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var sendLetter = new SendLetter(
			"sender",
			new[] { "receiver" },
			"test",
			"aabbcc",
			null,
			DateTime.UtcNow);

		// Act
		await sut.DeliveryAsync(sendLetter);

		// Assert
		_ = fakeMailSoltRepository.Received(1)
			.SaveSendLetterAsync(Arg.Is(sendLetter));
		_ = fakeMessageSender.Received(1)
			.PublishAsync(Arg.Is("tgs.station-letter.delivery"), Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Fact]
	public async Task 取得儲存的排程要求資料()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var sendLetterId = Guid.NewGuid();

		// Act
		_ = await sut.GetSendLetterAsync(sendLetterId);

		// Assert
		_ = fakeMailSoltRepository.Received(1)
			.GetSendLetterAsync(Arg.Is(sendLetterId));
	}

	[Fact]
	public async Task 暫存要發送的信件()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var sendLetterId = Guid.NewGuid();
		var letter = new Letter(
			Guid.NewGuid(),
			"sender",
			"receiver",
			"test",
			null,
			"aabbcc",
			null,
			DateTime.UtcNow,
			DateTime.UtcNow,
			false,
			false);

		// Act
		await sut.ScheduleLetterAsync(sendLetterId, letter);

		// Assert
		_ = fakeMailSoltRepository.Received(1)
			.SaveLetterAsync(Arg.Is(sendLetterId), Arg.Is(letter));
	}

	[Fact]
	public async Task 從送信要求id取得待送信件()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var sendLetterId = Guid.NewGuid();

		// Act
		_ = await sut.GetWaitingSendLettersAsync(sendLetterId).ToArrayAsync();

		// Assert
		_ = fakeMailSoltRepository.Received(1)
			.GetLettersAsync(Arg.Is(sendLetterId), Arg.Is(10));
	}

	[Fact]
	public async Task 完成送信排程()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var sendLetterId = Guid.NewGuid();

		// Act
		await sut.CompleteSendLetterAsync(sendLetterId);

		// Assert
		_ = fakeMailSoltRepository.Received(1)
			.CleanUpSendLetterAsync(Arg.Is(sendLetterId));
	}

	[Fact]
	public async Task 取得要回信的信件資料()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(
			fakeMailSoltRepository,
			fakeTgsRepository,
			fakeMessageSender);

		var letterId = Guid.NewGuid();

		// Act
		_ = await sut.GetLetterAsync(letterId);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.GetLetterAsync(Arg.Is(letterId));
	}
}
