using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;


namespace backend.Services;

public class PostService {
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Post> _postCollection;
    public PostService(IOptions<MongoDBSettings> mongoDBSettings){
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.UserCollection);
        _postCollection = database.GetCollection<Post>(mongoDBSettings.Value.PostCollection);
    }

    public async Task CreateOnePostAsync(Post post){
        await _postCollection.InsertOneAsync(post);
        return;
    }

    public async Task<Post?> UpdatePost(string id, Post newPost){
        return await _postCollection.FindOneAndReplaceAsync(x => x._id == id, newPost);
    }

    public async Task<Post?> GetPostByID(string id){
        return await _postCollection.Find(x => x._id == id).FirstOrDefaultAsync();
    }    
    
    public async Task<User?> GetUsByid(string id){
        return await _userCollection.Find(x => x._id == id).FirstOrDefaultAsync();
    }

    public async Task DeletePostAsync(string id){
        FilterDefinition<Post> filter = Builders<Post>.Filter.Eq("_id", id);
        await _postCollection.DeleteOneAsync(filter);
        return;
    }

    public async Task<(List<Post>, List<User>)> Search(string searchQuery){
        FilterDefinition<Post> FilterPost = new BsonDocument
        {
            {"title", new BsonDocument("$ne", searchQuery)},
            {"message", new BsonDocument("$regex", searchQuery)}
        };

        FilterDefinition<User> FilterUser = new BsonDocument
        {
            {"name", new BsonDocument("$ne", searchQuery)},
            {"email", new BsonDocument("$regex", searchQuery)}
        };

        List<Post> posts = (await _postCollection.FindAsync(FilterPost)).ToList();
        List<User> users = (await _userCollection.FindAsync(FilterUser)).ToList();

        if(posts is null){
            posts = new List<Post>();
        } else if (users is null){
            users = new List<User>();
        }

        return (posts, users);
    }

    public Object Query(List<string> ides, int? queryPage)
    {
        var filter = Builders<Post>.Filter.In("creator", ides);

        var sort = Builders<Post>.Sort.Descending("_id");
        var find = _postCollection.Find(filter).Sort(sort);

        int curentPage = queryPage.GetValueOrDefault(1) == 0 ? 1 : queryPage.GetValueOrDefault(1);

        int perPage = 3;
        var numberOfPages = find.CountDocuments() / perPage;
         
        return new 
        {
            data = find.Skip((curentPage -1) * perPage).Limit(perPage).ToList(),
            numberOfPages,
            curentPage,
        };


    }

}

