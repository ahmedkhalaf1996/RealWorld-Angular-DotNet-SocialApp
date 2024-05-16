using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace backend.Services;

public class UserService {
    private readonly IMongoCollection<User> _userCollection;
    public UserService(IOptions<MongoDBSettings> mongoDBSettings){
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.UserCollection);
    }

    public async Task CreateAsync(User user) {
        await _userCollection.InsertOneAsync(user);
        return; 
    }

    public async Task<User?> GetUserByEmail(string email){
        return await _userCollection.Find(x => x.email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByID(string id) {
        return await _userCollection.Find(x => x._id == id).FirstOrDefaultAsync();
    }


    public async Task<User?> UpdateUser(string id, User newuser) {
        return await _userCollection.FindOneAndReplaceAsync(x => x._id ==id, newuser);
    }

    public async Task DeleteAsync(string id){
        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", id);
        await _userCollection.DeleteOneAsync(filter);
        return;
    }

}





