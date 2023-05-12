using Google.Protobuf;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using Tgs.StationLetter.BrandAllPlayers.MessageQueue;
using Tgs.StationLetter.Protos;
using Tgs.StationLetterService;
using Tgs.StationLetterService.Client;

namespace StationLetter.BrandAllPlayers.Core.UnitTests;

public class DispatchBrandAllPlayersLetterHandlerTests
{
	[Fact]
	public async Task 處理發送全部玩家要求()
	{
		// Arrange
		var fakeStationLetterClient = Substitute.For<IStationLetterClient>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

		var sut = new DispatchBrandAllPlayersLetterHandler(
			fakeStationLetterClient,
			fakePlayerServiceClient,
			fakeBrandServiceClient);

		var request = new DeliverLetterRequest
		{
			DeliverLetterId = Guid.NewGuid()
		};

		_ = fakeStationLetterClient.GetDeliveryLetterAsync(Arg.Is(request.DeliverLetterId.ToGuid()))
			.Returns(new DeliveryLetter(
				request.DeliverLetterId.ToGuid(),
				"GPI",
				ReceiverType.BrandAllPlayers,
				null,
				"ttt",
				"cccc",
				null,
				DateTime.UtcNow));

		var brandId = Guid.NewGuid();
		_ = fakeBrandServiceClient.GetBrandInfoByCodeAsync(Arg.Is("GPI"))
			.Returns(new BrandInfo
			{
				Id = brandId,
				Code = "GPI"
			});

		_ = fakePlayerServiceClient.QueryPlayerListAsync(Arg.Is(brandId), Arg.Any<PlayerListQuery>())
			.Returns(new[] {
				new PlayerListItem(
					"test1",
					"test1",
					null,
					null,
					null,
					null,
					null,
					PlayerStatus.Enabled,
					null),
				new PlayerListItem(
					"test2",
					"test2",
					null,
					null,
					null,
					null,
					null,
					PlayerStatus.Enabled,
					null),
				new PlayerListItem(
					"test3",
					"test3",
					null,
					null,
					null,
					null,
					null,
					PlayerStatus.Enabled,
					null)
			}.ToAsyncEnumerable());
		;

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeStationLetterClient.Received(1)
			.PostStationLetterAsync(Arg.Any<PostStationLetterRequest>());
	}
}
