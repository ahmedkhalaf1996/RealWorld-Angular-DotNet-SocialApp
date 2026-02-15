using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;


namespace backend.Services;

public class NotificationService : INotificationService
{
    private readonly IMongoCollection<Notification> _notificationCollection;

    public NotificationService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _notificationCollection = database.GetCollection<Notification>(mongoDBSettings.Value.NotificationCollection);
    }

    public virtual async Task CreateNotification(Notification notification)
    {
        await _notificationCollection.InsertOneAsync(notification);
        // TODO CAll RealTime Noficiation grpc up

        return;
    }

    public virtual async Task<List<Notification>> GetUserNotification(string uid)
    {
        var filter = Builders<Notification>.Filter
                    .Regex("mainuid", new BsonRegularExpression(uid, "i"));

        var pipeline = BuildNotificatonsWithUserPipeline(filter);

        var notifiactions = await _notificationCollection
                    .Aggregate<Notification>(pipeline)
                    .ToListAsync();

        return notifiactions;
    }

    private List<BsonDocument> BuildNotificatonsWithUserPipeline(FilterDefinition<Notification> matchFilter)
    {
        var pipeline = new List<BsonDocument>();

        pipeline.Add(new BsonDocument("$match", matchFilter.Render(new RenderArgs<Notification>(BsonSerializer.LookupSerializer<Notification>(), BsonSerializer.SerializerRegistry))));

        pipeline.Add(new BsonDocument("$sort", new BsonDocument("createdAt", -1)));

        pipeline.Add(new BsonDocument("$addFields", new BsonDocument
        {
            {
                "senderidAsObjectId", new BsonDocument("$cond", new BsonArray
                {
                    new BsonDocument("$eq", new BsonArray {new BsonDocument("$type", "$senderid"), "objectId"}),
                    "$senderid", new BsonDocument("$toObjectId", "$senderid")
                })
            }
        }));

        //
        pipeline.Add(new BsonDocument("$lookup", new BsonDocument
        {
            {"from", "users"},
            {"let", new BsonDocument("uid", "$senderidAsObjectId")},
            {"pipeline", new BsonArray
            {
                new BsonDocument("$match", new BsonDocument("$expr",
                    new BsonDocument("$eq", new BsonArray {"$_id", "$$uid"}))),
                new BsonDocument("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"name", 1},
                    {"imageUrl", 1}
                })
            }
            },
            {"as", "user"}
        }));

        pipeline.Add(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$user" },
                { "preserveNullAndEmptyArrays", true }
            }));

        pipeline.Add(new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "deatils", 1 },
                    { "mainuid", 1 },
                    { "targetid", 1 },
                    { "isreded", 1 },
                    { "createdAt", 1 },
                    { "user", 1}
                }));
        return pipeline;

    }

    public virtual async Task<bool> MarkNotificationsAsReaded(string uid)
    {
        var filter = Builders<Notification>.Filter
                    .Regex("mainuid", new BsonRegularExpression(uid, "i"));
        var update = Builders<Notification>.Update
                .Set(x => x.isreded, true);

        var result = await _notificationCollection.UpdateManyAsync(filter, update);

        if (result == null) { return false; } else { return true; }
    }
}

