using JetBrains.Annotations;
using Titeenipeli.Schema;
using NUnit.Framework;
using FluentAssertions;
using Titeenipeli.Grpc.Controllers;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Common;
using System.Collections.Concurrent;
using GrpcGeneratedServices;
using Titeenipeli.Options;
using System.Collections.Generic;
using Titeenipeli.Models;
using System.Collections;
using System.Threading.Channels;
using System.Threading.Tasks;
using Titeenipeli.Grpc.Services;
using Moq;

namespace Titeenipeli.Tests.Grpc;

[TestSubject(typeof(MapUpdateProcessor))]
public class MapUpdateProcessorTest
{
    public const int Emp = 0;
    public const int Own = 1;
    public const int Oth = 2;
    public const int Nop = 3;
    public const int Bor = 4;

    const int Width = 10;
    const int Height = 10;
    const int FogOfWarDistance = 2;

    private IGrpcConnection<IncrementalMapUpdateResponse> _connection;
    private ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _connections;
    private IIncrementalMapUpdateCoreService _incrementalMapUpdateCoreService;
    private GameOptions _gameOptions;

    private static Guild _ownGuild = new()
    {
        Name = Enums.GuildName.Cluster
    };
    private static Guild _otherGuild = new()
    {
        Name = Enums.GuildName.Tietokilta
    };
    private static User _currentUser = new()
    {
        Id = 1,
        Guild = _ownGuild,
        Code = "",
        SpawnX = 0,
        SpawnY = 0,
        TelegramId = "",
        FirstName = "Own user",
        LastName = "",
        Username = "",
        PhotoUrl = "",
        AuthDate = "",
        Hash = ""
    };
    private static User _otherUser = new()
    {
        Id = 2,
        Guild = _otherGuild,
        Code = "",
        SpawnX = 0,
        SpawnY = 0,
        TelegramId = "",
        FirstName = "Other user",
        LastName = "",
        Username = "",
        PhotoUrl = "",
        AuthDate = "",
        Hash = ""
    };
    private static List<User> _users = new() {
        _currentUser,
        _otherUser
    };

    [SetUp]
    public void Init()
    {
        _connections = [];
        _connection = new GrpcConnectionMock<IncrementalMapUpdateResponse>
        {
            User = _currentUser
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
    public async Task GrpcMapUpdateProcessorTests(int[,] inputMap, List<GrpcMapChangeInput> changes, int[,] outputMap)
    {
        Dictionary<Coordinate, GrpcChangePixel> newPixels = MapUtils.MatrixOfUsersToPixels(inputMap, _users);
        GrpcMapChangesInput input = new(newPixels, changes);
        MapUpdateProcessor mapUpdateProcessor = new(_incrementalMapUpdateCoreService, input, _connections, _gameOptions);

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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 5}, null, _otherUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 5}, null, _currentUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 5}, _currentUser, _otherUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 6}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 3, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 4, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 6, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 7, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 2}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 3}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 4}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 5}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 6}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 7}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 8, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 7, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 6, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 4, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 3, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 8}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 7}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 6}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 5}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 4}, null, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 3}, null, _otherUser),
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 5, Y = 5}, _currentUser, _otherUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 0, Y = 0}, null, _currentUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 0, Y = 0}, _currentUser, _otherUser)
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
                new List<GrpcMapChangeInput>() {
                    new GrpcMapChangeInput(new Coordinate() {X = 1, Y = 1}, _currentUser, _otherUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 2, Y = 1}, _otherUser, _currentUser),
                    new GrpcMapChangeInput(new Coordinate() {X = 1, Y = 2}, null, _otherUser),
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