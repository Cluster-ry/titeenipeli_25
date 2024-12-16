using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using GrpcGeneratedServices;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Controllers.Grpc;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using Titeenipeli.Options;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Tests.Grpc;

[TestSubject(typeof(MapUpdateProcessor))]
public class MapUpdateProcessorTest
{
    // Empty pixel.
    public const int Emp = 0;
    // Pixel owned by current user.
    public const int Own = 1;
    // Pixel owned by other user.
    public const int Oth = 2;
    // No pixel data send. Available only on output.
    public const int Nop = 3;
    // Pixel reprsenting border. Available only on output.
    public const int Bor = 4;

    private const int Width = 10;
    private const int Height = 10;
    private const int FogOfWarDistance = 2;

    private IGrpcConnection<IncrementalMapUpdateResponse> _connection;
    private ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _connections;
    private IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService;
    private GameOptions _gameOptions;
    private IBackgroundGraphicsService _backgroundGraphicsService = new Mock<IBackgroundGraphicsService>().Object;

    private static readonly Guild OwnGuild = new()
    {
        Name = GuildName.Cluster,
        ActiveCtfFlags = []
    };
    private static readonly Guild OtherGuild = new()
    {
        Name = GuildName.Tietokilta,
        ActiveCtfFlags = []
    };
    private static readonly User CurrentUser = new()
    {
        Id = 1,
        Guild = OwnGuild,
        Code = "",
        SpawnX = 0,
        SpawnY = 0,
        PowerUps = [],
        TelegramId = "",
        FirstName = "Own user",
        LastName = "",
        Username = ""
    };

    private static readonly User OtherUser = new()
    {
        Id = 2,
        Guild = OtherGuild,
        Code = "",
        SpawnX = 0,
        SpawnY = 0,
        PowerUps = [],
        TelegramId = "",
        FirstName = "Other user",
        LastName = "",
        Username = ""
    };

    private static readonly List<User> Users =
    [
        CurrentUser,
        OtherUser
    ];

    [SetUp]
    public void Init()
    {
        _connections = [];
        _connection = new GrpcConnectionMock<IncrementalMapUpdateResponse>
        {
            User = CurrentUser
        };
        _connections.TryAdd(1, _connection);
        _gameOptions = new GameOptions()
        {
            Width = Width,
            Height = Height,
            FogOfWarDistance = FogOfWarDistance
        };
        _incrementalMapUpdateCoreService = new Mock<IIncrementalMapUpdateCoreService>().Object;
    }

    [TearDown]
    public void Dispose()
    {
        _connection.Dispose();
    }


    [TestCaseSource(nameof(IncrementalMapUpdateTestCases))]
    public async Task GrpcMapUpdateProcessorTests(int[,] inputMap, List<MapChange> changes, int[,] outputMap)
    {
        var newPixels = MapUtils.MatrixOfUsersToPixels(inputMap, Users);
        GrpcMapChangesInput input = new(newPixels, changes);
        MapUpdateProcessor mapUpdateProcessor = new(_incrementalMapUpdateCoreService, input, _connections, _gameOptions, _backgroundGraphicsService);

        await mapUpdateProcessor.Process();

        IncrementalMapUpdateResponse response = await _connection.ResponseStreamQueue.Reader.ReadAsync();
        int[,] results = MapUtils.GrpcUpdatesToUserMap(response.Updates, _gameOptions);
        results.Should().BeEquivalentTo(outputMap);
    }

    public static IEnumerable IncrementalMapUpdateTestCases
    {
        get
        {
            yield return new TestCaseData(
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Own, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 5, Y = 5 }, null, OtherUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Oth, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should update pixel when inside field of view");
            yield return new TestCaseData(
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Own, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new MapChange(new Coordinate {X = 5, Y = 5}, null, CurrentUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Oth, Oth, Oth, Oth, Oth, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Oth, Oth, Oth, Oth, Oth, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Oth, Oth, Own, Oth, Oth, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Oth, Oth, Oth, Oth, Oth, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Oth, Oth, Oth, Oth, Oth, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should update nearby pixels when user wins pixel");
            yield return new TestCaseData(
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 5, Y = 5 }, CurrentUser, OtherUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should update nearby pixels when user loses pixel");
            yield return new TestCaseData(
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Emp, Emp, Emp, Emp, Emp, Oth, Emp },
                    { Emp, Emp, Oth, Emp, Emp, Emp, Emp, Emp, Oth, Emp },
                    { Emp, Emp, Oth, Emp, Emp, Own, Emp, Emp, Oth, Emp },
                    { Emp, Emp, Oth, Emp, Emp, Oth, Emp, Emp, Oth, Emp },
                    { Emp, Emp, Oth, Emp, Emp, Emp, Emp, Emp, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 5, Y = 6 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 3, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 4, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 5, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 6, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 7, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 2 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 3 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 4 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 5 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 6 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 7 }, null, OtherUser),
                    new(new Coordinate { X = 8, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 7, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 6, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 5, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 4, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 3, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 8 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 7 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 6 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 5 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 4 }, null, OtherUser),
                    new(new Coordinate { X = 2, Y = 3 }, null, OtherUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Oth, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should not update pixels when outside of field of view");
            yield return new TestCaseData(
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Own, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 5, Y = 5 }, CurrentUser, OtherUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Oth, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should not remove visibility from pixels still inside field of view when user loses pixel");
            yield return new TestCaseData(
                new[,]
                {
                    { Own, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                },
                new List<MapChange> {
                    new MapChange(new Coordinate {X = 0, Y = 0}, null, CurrentUser)
                },
                new[,]
                {
                    { Bor, Bor, Bor, Bor, Bor, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Bor, Bor, Bor, Bor, Bor, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Bor, Bor, Own, Oth, Oth, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Bor, Bor, Oth, Oth, Oth, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Bor, Bor, Oth, Oth, Oth, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should handle borders when pixel win happens near border");
            yield return new TestCaseData(
                new[,]
                {
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                    { Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth, Oth },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 0, Y = 0 }, CurrentUser, OtherUser)
                },
                new[,]
                {
                    { Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should handle borders when pixel loss happens near border");
            yield return new TestCaseData(
                new[,]
                {
                    { Own, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Oth, Own, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Oth, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                },
                new List<MapChange> {
                    new(new Coordinate { X = 1, Y = 1 }, CurrentUser, OtherUser),
                    new(new Coordinate { X = 2, Y = 1 }, OtherUser, CurrentUser),
                    new(new Coordinate { X = 1, Y = 2 }, null, OtherUser)
                },
                new[,]
                {
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Bor, Bor, Bor, Bor, Bor, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Own, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Emp, Oth, Own, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Emp, Oth, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Emp, Emp, Emp, Emp, Emp, Emp, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                    { Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop, Nop  },
                }).SetName("Should handle all updates when multiple changes occur");
        }
    }

    private class GrpcConnectionMock<TResponseStream> : IGrpcConnection<TResponseStream>
    {
        public int Id { get; init; }
        public User User { get; set; }
        public Channel<TResponseStream> ResponseStreamQueue { get; init; } = Channel.CreateBounded<TResponseStream>(1);
        public Task ProcessResponseWritesTask { get; init; }
        public void Dispose()
        {

        }
    }
}