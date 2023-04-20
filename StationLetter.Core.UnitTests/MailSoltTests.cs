using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Tgs.MessageQueue;
using Tgs.StationLetter;
using Xunit;

namespace StationLetter.Core.UnitTests;
public class MailSoltTests
{
	[Fact]
	public async Task 排程發送要求()
	{
		// Arrange
		var fakeMailSoltRepository = Substitute.For<IMailSlotRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(fakeMailSoltRepository, fakeMessageSender);

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
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(fakeMailSoltRepository, fakeMessageSender);

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
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new MailSlot(fakeMailSoltRepository, fakeMessageSender);

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
}
