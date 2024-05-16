using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Services;

public class ChatService {
    private readonly IMongoCollection<UnReadedMessages> _unReadedmessageCollection;
    private readonly IMongoCollection<Message> _messageCollection;
    private readonly IMongoCollection<User> _userCollection;

    public ChatService(IOptions<MongoDBSettings> mongoDBSettings){
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _unReadedmessageCollection = database.GetCollection<UnReadedMessages>(mongoDBSettings.Value.UnMessageCollection);
        _messageCollection = database.GetCollection<Message>(mongoDBSettings.Value.MessageCollection);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.UserCollection);
    }


    
    //
    public async Task SendMessageAsync(Message msg, string sender, string recever){
        await _messageCollection.InsertOneAsync(msg);

        setUpdateUnreadedMessageBetweenUsers(sender, recever);
        return;
    }

    public async void setUpdateUnreadedMessageBetweenUsers(string sender, string recever){
        var filter = Builders<UnReadedMessages>.Filter.And(
            Builders<UnReadedMessages>.Filter.Eq(x => x.mainUserid, recever),
            Builders<UnReadedMessages>.Filter.Eq(x => x.otherUserid, sender)
        );

        var update = Builders<UnReadedMessages>.Update
                .Set(x => x.isReaded, false)
                .Inc(x => x.numOfUnreadedMessages, 1);

        var options = new FindOneAndUpdateOptions<UnReadedMessages>{
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _unReadedmessageCollection.FindOneAndUpdateAsync(filter, update, options);

        if(result == null ){
            var newUnrededMsg = new UnReadedMessages{
                mainUserid = recever,
                otherUserid = sender,
                isReaded = false,
                numOfUnreadedMessages = 1
            };

            await _unReadedmessageCollection.InsertOneAsync(newUnrededMsg);
        }
    }

    public async Task<List<Message>> GetMessageByNum(int from, string firstuid, string seconduid){
        var senderFilter = Builders<Message>.Filter.Eq("sender", firstuid);
        var receverFilter = Builders<Message>.Filter.Eq("recever", seconduid);

        var senderFilter1 = Builders<Message>.Filter.Eq("recever", firstuid);
        var receverFilter1 = Builders<Message>.Filter.Eq("sender", seconduid);
        
        var combinedFiter = Builders<Message>.Filter.Or(
            Builders<Message>.Filter.And(senderFilter, receverFilter),
            Builders<Message>.Filter.And(senderFilter1, receverFilter1)
        );

        var sort = Builders<Message>.Sort.Descending("_id");
        var numOfReturningMessages = 8;
        var messages = await _messageCollection
            .Find(combinedFiter)
            .Sort(sort)
            .Skip(from * numOfReturningMessages)
            .Limit(numOfReturningMessages)
            .ToListAsync();

        messages.Reverse();

        return messages;

    }


    public async Task<List<UnReadedMessages>> GetUserUnreadedmsgs(string userid)
    {
        var filter1 = Builders<UnReadedMessages>.Filter.Eq("mainUserid", userid);
        var filter2 = Builders<UnReadedMessages>.Filter.Eq("isReaded", false);

        var combidedFilter = Builders<UnReadedMessages>.Filter.And(filter1, filter2);

        var urms = await _unReadedmessageCollection.Find(combidedFilter).ToListAsync();

        return urms;
    }

    public async Task<bool> MarkMsgsAsReaded(string otheruid, string mainuid)
    {
        var filter = Builders<UnReadedMessages>.Filter.And(
            Builders<UnReadedMessages>.Filter.Eq(x => x.mainUserid , mainuid),
            Builders<UnReadedMessages>.Filter.Eq(x => x.otherUserid , otheruid)
        );

        var update = Builders<UnReadedMessages>.Update
            .Set(x => x.isReaded ,true)
            .Set(x=> x.numOfUnreadedMessages, 0);
        
        var options = new FindOneAndUpdateOptions<UnReadedMessages>{
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _unReadedmessageCollection.FindOneAndUpdateAsync(filter, update, options); 

        if(result == null){
            return false;
        } else {
            return true;
        }

    }



}

