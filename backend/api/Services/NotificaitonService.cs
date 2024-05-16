using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;


namespace backend.Services;

public class NotificationService {
    private readonly IMongoCollection<Notification> _notificationCollection;

    public NotificationService(IOptions<MongoDBSettings> mongoDBSettings){
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _notificationCollection = database.GetCollection<Notification>(mongoDBSettings.Value.NotificationCollection);
    }

    public async Task CreateNotification(Notification notification){
        await _notificationCollection.InsertOneAsync(notification);
        // TODO CAll RealTime Noficiation grpc

        return; 
    }

    public async Task<List<Notification>> GetUserNotification(string uid){
        var filter = Builders<Notification>.Filter
                    .Regex("mainuid", new BsonRegularExpression(uid, "i"));
        
        var notifiactions = await _notificationCollection
                    .Find(filter)
                    .SortByDescending(p => p.createdAt)
                    .ToListAsync();

        return notifiactions;
    }

    public  async Task<bool> MarkNotificationsAsReaded(string uid){
        var filter = Builders<Notification>.Filter
                    .Regex("mainuid", new BsonRegularExpression(uid, "i"));
        var update = Builders<Notification>.Update
                .Set(x => x.isreded, true);

        var result = await _notificationCollection.UpdateManyAsync(filter, update);

        if (result == null) {return false;} else {return true;} 
    }
}

