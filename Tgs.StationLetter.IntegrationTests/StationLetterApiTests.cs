using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tgs.StationLetterService.Client;
using Tgs.StationLetterService.Repositories.Entities;
using Xunit.Abstractions;

namespace Tgs.StationLetterService.IntegrationTests;

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

	[Fact]
	public async Task 取得User的站內信列表()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test2",
				Receiver = user,
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow,
				ReceivedOn = DateTime.UtcNow
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test3",
				Receiver = user,
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow,
				ReceivedOn = DateTime.UtcNow
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test4",
				Receiver = "test5",
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow,
				ReceivedOn = DateTime.UtcNow
			}
		};
		await dbContext.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows.Take(2)
			.Select(row => new Client.StationLetter(
				row.Id,
				row.Sender,
				row.Receiver,
				row.Subject,
				row.Quotation,
				row.Content,
				row.ComeFrom,
				row.SentOn,
				row.HaveRead,
				row.Replied))
			.ToArray();

		// Act
		var actual = await sut.ListStationLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject().ShouldMatch(actual);
	}

	[Fact]
	public async Task 取得User的站內信列表_只能列出90天的資料()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test2",
				Receiver = user,
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow.AddDays(-10D),
				ReceivedOn = DateTime.UtcNow.AddDays(-10D)
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test3",
				Receiver = user,
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow.AddDays(-50D),
				ReceivedOn = DateTime.UtcNow.AddDays(-50D)
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test4",
				Receiver = user,
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow.AddDays(-91D),
				ReceivedOn = DateTime.UtcNow.AddDays(-91D)
			}
		};
		await dbContext.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows.Take(2)
			.Select(row => new Client.StationLetter(
				row.Id,
				row.Sender,
				row.Receiver,
				row.Subject,
				row.Quotation,
				row.Content,
				row.ComeFrom,
				row.SentOn,
				row.HaveRead,
				row.Replied))
			.ToArray();

		// Act
		var actual = await sut.ListStationLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject().ShouldMatch(actual);
	}

	[Fact]
	public async Task 取得User的站內信列表_會依照SentOn排序()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test4",
				Receiver = user,
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow.AddDays(-81D),
				ReceivedOn = DateTime.UtcNow.AddDays(-81D)
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test3",
				Receiver = user,
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow.AddDays(-50D),
				ReceivedOn = DateTime.UtcNow.AddDays(-50D)
			},
			new StationLetterRow
			{
				Id = Guid.NewGuid(),
				Sender = "test2",
				Receiver = user,
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow.AddDays(-10D),
				ReceivedOn = DateTime.UtcNow.AddDays(-10D)
			}
		};
		await dbContext.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows
			.Select(row => new Client.StationLetter(
				row.Id,
				row.Sender,
				row.Receiver,
				row.Subject,
				row.Quotation,
				row.Content,
				row.ComeFrom,
				row.SentOn,
				row.HaveRead,
				row.Replied))
			.OrderByDescending(latter => latter.SentOn)
			.ToArray();

		// Act
		var actual = await sut.ListStationLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject().ShouldMatch(actual);
	}

	[Fact]
	public async Task 標記站內信已讀()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var letterId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var row = new StationLetterRow
		{
			Id = letterId,
			Sender = "test4",
			Receiver = "test1",
			Subject = "t3",
			Content = "t3t3",
			SentOn = DateTime.UtcNow.AddDays(-81D),
			ReceivedOn = DateTime.UtcNow.AddDays(-81D)
		};
		_ = await dbContext.StationLetterRows.AddAsync(row);

		_ = await dbContext.SaveChangesAsync();

		// Act
		Assert.False(row.HaveRead);

		await sut.MarkStationLetterHaveReadAsync(letterId);

		// Assert
		Assert.True(row.HaveRead);
	}
}
