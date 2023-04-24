using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tgs.StationLetter.Client;
using Xunit.Abstractions;

namespace Tgs.StationLetter.IntegrationTests;

public class StationLetterApiTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public StationLetterApiTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	}

	[Fact]
	public async Task 送出站內信()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();

		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddSingleton(fakeMailSlot)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var request = new StationLetterRequest(
			"sender",
			new[] { "receiver" },
			"abc",
			"aabbcc");

		var expected = new
		{
			request.Sender,
			request.Receivers,
			request.Subject,
			request.Content
		}.ToExpectedObject();

		// Act
		await sut.SendStationLetterAsync(request);

		// Assert
		_ = fakeMailSlot.Received(1)
			.DeliveryAsync(
				Arg.Is<SendLetter>(l => expected.Matches(l)),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 回覆站內信()
	{
		// Arrange
		var fakeMailSlot = Substitute.For<IMailSlot>();

		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddSingleton(fakeMailSlot)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var letterId = Guid.NewGuid();
		var request = new ReplyStationLetterRequest("receiver", "sender", "987654321");

		var expected = new
		{
			request.Sender,
			Receivers = new[] { request.Receiver },
			Subject = (string?)null,
			request.Content,
			ComeFrom = letterId
		}.ToExpectedObject();

		// Act
		await sut.ReplyStationLetterAsync(letterId, request);

		// Assert
		_ = fakeMailSlot.Received(1)
			.DeliveryAsync(
				Arg.Is<SendLetter>(l => expected.Matches(l)),
				Arg.Any<CancellationToken>());
	}
}
