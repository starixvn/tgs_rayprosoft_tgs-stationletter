using NSubstitute;
using Tgs.StationLetterService;

namespace StationLetter.Core.UnitTests;

public class StationLetterPostmanTests
{
	[Fact]
	public async Task 發送信件()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new StationLetterPostman(fakeTgsRepository);

		var letters = new[]
		{
			new Letter(
				Guid.NewGuid(),
				"sender1",
				"receiver1",
				"test",
				null,
				"aabbcc",
				null,
				DateTime.UtcNow,
				DateTime.UtcNow,
				false,
				false),
			new Letter(
				Guid.NewGuid(),
				"sender2",
				"receiver2",
				"test123",
				null,
				"aabbccdd",
				null,
				DateTime.UtcNow,
				DateTime.UtcNow,
				false,
				false)
		};

		// Act
		await sut.InboxAsync(letters);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.InsertStationLettersAsync(Arg.Is(letters));
	}
}
