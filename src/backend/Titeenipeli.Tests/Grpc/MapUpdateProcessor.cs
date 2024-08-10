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

namespace Titeenipeli.Tests.Grpc;

[TestSubject(typeof(MapUpdateProcessor))]
public class MapUpdateProcessorTest
{
    const int Emp = 0;
    const int Own = 1;
    const int Oth = 2;

    private IGrpcConnection<IncrementalMapUpdateResponse> _connection;
    private ConcurrentDictionary<int, IGrpcConnection<IncrementalMapUpdateResponse>> _connections;
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
        FluentAssertions.Formatting.Formatter.AddFormatter(new IntMatrixFormatter());

        _connections = [];
        _connection = new GrpcConnectionMock<IncrementalMapUpdateResponse>
        {
            User = _currentUser
        };
        _connections.TryAdd(1, _connection);
        _gameOptions = new GameOptions()
        {
            Width = 10,
            Height = 10,
            FogOfWarDistance = 2
        };
    }

    [TestCaseSource(nameof(IncrementalMapUpdateTestCases))]
    public async Task GrpcMapUpdateProcessorTests(int[,] inputMap, List<GrpcMapChangeInput> changes, int[,] outputMap)
    {
        Dictionary<Coordinate, GrpcChangePixel> oldPixels = MapUtils.MatrixOfUsersToPixels(inputMap, _users);
        GrpcMapChangesInput input = new(oldPixels, changes);
        MapUpdateProcessor mapUpdateProcessor = new(input, _connections, _gameOptions);

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
                    { Emp, Emp, Oth, Oth, Oth, Emp, Oth, Oth, Oth, Emp },
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
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Oth, Oth, Oth, Oth, Oth, Emp, Emp },
                    { Emp, Emp, Emp, Oth, Oth, Oth, Oth, Oth, Emp, Emp },
                    { Emp, Emp, Emp, Oth, Oth, Own, Oth, Oth, Emp, Emp },
                    { Emp, Emp, Emp, Oth, Oth, Oth, Oth, Oth, Emp, Emp },
                    { Emp, Emp, Emp, Oth, Oth, Oth, Oth, Oth, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                    { Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp, Emp },
                }).SetName("Should update nearby pixels when user wins pixel");
        }
    }

    private class GrpcConnectionMock<TResponseStream> : IGrpcConnection<TResponseStream>
    {
        public int Id { get; init; }
        public User User { get; set; }
        public Channel<TResponseStream> ResponseStreamQueue { get; init; } = Channel.CreateBounded<TResponseStream>(1);
    }
}