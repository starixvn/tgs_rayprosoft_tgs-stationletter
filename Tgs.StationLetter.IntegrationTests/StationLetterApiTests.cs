using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using Tgs.StationLetterService.ApplicationServices;
using Tgs.StationLetterService.Client;
using Tgs.StationLetterService.Repositories.Entities;
using Xunit.Abstractions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
		var fakeSendStationLetterCommandService = Substitute.For<ICommandService<SendStationLetterCommand>>();

		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddSingleton(fakeSendStationLetterCommandService)));

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
		_ = fakeSendStationLetterCommandService.Received(1)
			.ExecuteAsync(
				Arg.Is<SendStationLetterCommand>(cmd => expected.Matches(cmd)),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 回覆站內信()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeMessageSender = Substitute.For<IMessageSender>();

		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddSingleton(fakeTgsRepository)
					.AddSingleton(fakeMessageSender)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var letterId = Guid.NewGuid();
		var request = new ReplyStationLetterRequest("receiver", "sender", "987654321");

		_ = fakeTgsRepository.GetLetterAsync(Arg.Is(letterId), Arg.Any<CancellationToken>())
			.Returns(new Letter(
				letterId,
				request.Receiver,
				request.Sender,
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
			request.Sender,
			ReceiverType = ReceiverType.Receivers,
			Subject = "Re:ttt",
			request.Content,
			ComeFrom = letterId
		}.ToExpectedObject();

		// Act
		await sut.ReplyStationLetterAsync(letterId, request);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.InsertDeliveryLetterAsync(Arg.Is<DeliveryLetter>(
				delivery => expected.Matches(delivery)), Arg.Any<CancellationToken>());
		_ = fakeMessageSender.Received(1)
			.PublishAsync(
				Arg.Is("tgs.station-letter.dispatch.receivers"),
				Arg.Any<ReadOnlyMemory<byte>>(),
				Arg.Any<CancellationToken>());
		_ = fakeTgsRepository.Received(1)
			.SetLetterRepliedAsync(Arg.Is(letterId), Arg.Any<CancellationToken>());
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
		Assert.Equal(expected, actual);
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

	[Fact]
	public async Task 取得站內信內容()
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

		var expected = new Client.StationLetter(
			letterId,
			row.Sender,
			row.Receiver,
			row.Subject,
			row.Quotation,
			row.Content,
			row.ComeFrom,
			row.SentOn,
			row.HaveRead,
			row.Replied);

		// Act
		var actual = await sut.GetStationLetterAsync(letterId);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task 取得User的寄件列表()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddHttpClientLoggingHandler()));

		var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<StationLetterClient>());

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test2"}),
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test3"}),
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test4"}),
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow
			}
		};
		await dbContext.StationLetterDeliveries.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows
			.Select(row => new Client.DeliveryLetter(
				row.Id,
				row.Sender,
				Enum.Parse<ReceiverType>(row.ReceiverType),
				JsonDocument.Parse(row.Receiver!),
				row.Subject,
				row.Content,
				row.ComeFrom,
				row.SentOn))
			.ToArray();

		// Act
		var actual = await sut.ListDeliverLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject(ctx => ctx.ArrayMemberShouldEqual(
			o => o,
			itemCtx =>
			{
				_ = itemCtx.IgnoreEqualsOverride();
				_ = itemCtx.IgnoreRelativePath(nameof(DeliveryLetter.Receiver));
			})).ShouldMatch(actual);
	}

	[Fact]
	public async Task 取得User的寄件列表_會依照SentOn排序()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddHttpClientLoggingHandler()));

		var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<StationLetterClient>());

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test2"}),
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow.AddDays(-81D)
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test3"}),
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow.AddDays(-51D)
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test4"}),
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow.AddDays(-10D)
			}
		};
		await dbContext.StationLetterDeliveries.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows
			.Select(row => new Client.DeliveryLetter(
				row.Id,
				row.Sender,
				Enum.Parse<ReceiverType>(row.ReceiverType),
				JsonDocument.Parse(row.Receiver!),
				row.Subject,
				row.Content,
				row.ComeFrom,
				row.SentOn))
			.OrderByDescending(row => row.SentOn)
			.ToArray();

		// Act
		var actual = await sut.ListDeliverLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject(ctx => ctx.ArrayMemberShouldEqual(
			o => o,
			itemCtx =>
			{
				_ = itemCtx.IgnoreEqualsOverride();
				_ = itemCtx.IgnoreRelativePath(nameof(DeliveryLetter.Receiver));
			})).ShouldEqual(actual);
		Assert.Equal(
			expected.Select(item => item.SentOn).ToArray(),
			actual.Select(item => item.SentOn).ToArray());
	}

	[Fact]
	public async Task 取得User的寄件列表_只能列出90天的資料()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
				.ConfigureServices(services => services
					.AddHttpClientLoggingHandler()));

		var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<StationLetterClient>());

		var sut = new StationLetterClient(httpClient);

		var user = "test1";

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var rows = new[] {
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test2"}),
				Subject = "t1",
				Content = "t1t1",
				SentOn = DateTime.UtcNow.AddDays(-91D)
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test3"}),
				Subject = "t2",
				Content = "t2t2",
				SentOn = DateTime.UtcNow.AddDays(-51D)
			},
			new StationLetterDelivery
			{
				Id = Guid.NewGuid(),
				Sender = user,
				ReceiverType = ReceiverType.Receivers.ToString(),
				Receiver = JsonSerializer.Serialize(new[]{ "test4"}),
				Subject = "t3",
				Content = "t3t3",
				SentOn = DateTime.UtcNow.AddDays(-10D)
			}
		};
		await dbContext.StationLetterDeliveries.AddRangeAsync(rows);

		_ = await dbContext.SaveChangesAsync();

		var expected = rows.Skip(1)
			.Select(row => new Client.DeliveryLetter(
				row.Id,
				row.Sender,
				Enum.Parse<ReceiverType>(row.ReceiverType),
				JsonDocument.Parse(row.Receiver!),
				row.Subject,
				row.Content,
				row.ComeFrom,
				row.SentOn))
			.OrderByDescending(row => row.SentOn)
			.ToArray();

		// Act
		var actual = await sut.ListDeliverLettersAsync(user).ToArrayAsync();

		// Assert
		expected.ToExpectedObject(ctx => ctx.ArrayMemberShouldEqual(
			o => o,
			itemCtx =>
			{
				_ = itemCtx.IgnoreEqualsOverride();
				_ = itemCtx.IgnoreRelativePath(nameof(DeliveryLetter.Receiver));
			})).ShouldEqual(actual);
		Assert.Equal(
			expected.Select(item => item.SentOn).ToArray(),
			actual.Select(item => item.SentOn).ToArray());
	}

	[Fact]
	public async Task 取得寄件備份內容()
	{
		// Arrange
		await using var web = StationLetterTestHost.Factory
			.WithWebHostBuilder(hostBuilder => hostBuilder
				.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper)));

		var httpClient = web.CreateClient();

		var sut = new StationLetterClient(httpClient);

		var letterId = Guid.NewGuid();

		var dbContext = web.Services.GetRequiredService<TgsContext>();

		var row = new StationLetterDelivery
		{
			Id = letterId,
			Sender = "test4",
			ReceiverType = ReceiverType.Receivers.ToString(),
			Receiver = """["test1"]""",
			Subject = "t3",
			Content = "t3t3",
			SentOn = DateTime.UtcNow.AddDays(-81D),
		};
		_ = await dbContext.StationLetterDeliveries.AddAsync(row);

		_ = await dbContext.SaveChangesAsync();

		var expected = new Client.DeliveryLetter(
			letterId,
			row.Sender,
			Enum.Parse<ReceiverType>(row.ReceiverType),
			JsonDocument.Parse(row.Receiver),
			row.Subject,
			row.Content,
			row.ComeFrom,
			row.SentOn);

		// Act
		var actual = await sut.GetDeliveryLetterAsync(letterId);

		// Assert
		expected.ToExpectedObject(ctx =>
		{
			_ = ctx.IgnoreEqualsOverride();
			_ = ctx.IgnoreRelativePath(nameof(DeliveryLetter.Receiver));
		}).ShouldEqual(actual);
	}
}
