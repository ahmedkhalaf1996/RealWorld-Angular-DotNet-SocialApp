using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;


namespace backend.Services;

public class PostService : IPostService {
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Post> _postCollection;

    private readonly IMongoCollection<Comment> _commentCollection;
    public PostService(IOptions<MongoDBSettings> mongoDBSettings){
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.UserCollection);
        _postCollection = database.GetCollection<Post>(mongoDBSettings.Value.PostCollection);
        _commentCollection = database.GetCollection<Comment>(mongoDBSettings.Value.CommentCollection);
    }

    public virtual async Task CreateOnePostAsync(Post post){
        await _postCollection.InsertOneAsync(post);
        return;
    }

    public virtual async Task<Post?> UpdatePost(string id, Post newPost){
        return await _postCollection.FindOneAndReplaceAsync(x => x._id == id, newPost);
    }

    public virtual async Task<PostResponse?> GetPostByID(string id){
        var objectId =  ObjectId.Parse(id);
        var filter = Builders<Post>.Filter.Eq("_id", objectId);
        var pipeline = BuildPostWithCommentsAndUserPipeline(filter);

        var result = await _postCollection.Aggregate<PostResponse>(pipeline).FirstOrDefaultAsync();
        return result;
    }    
    
    public virtual async Task<User?> GetUsByid(string id){
        return await _userCollection.Find(x => x._id == id).FirstOrDefaultAsync();
    }

    public virtual async Task DeletePostAsync(string id){
        FilterDefinition<Post> filter = Builders<Post>.Filter.Eq("_id", id);
        await _postCollection.DeleteOneAsync(filter);
        return;
    }

    public virtual async Task<(List<Post>, List<User>)> Search(string searchQuery){
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

    public virtual async Task<Object> Query(List<string> ides, int? queryPage)
    {

        var filterDef = new BsonDocument("$or", new BsonArray
        {
            new BsonDocument("creator", new BsonDocument("$in", new BsonArray(ides.Select(id => ObjectId.Parse(id)).ToList()))),
            new BsonDocument("creator", new BsonDocument("$in", new BsonArray(ides)))
        });

        int curentPage = queryPage.GetValueOrDefault(1) == 0 ? 1 : queryPage.GetValueOrDefault(1);

        int perPage = 3;
        var total = await _postCollection.CountDocumentsAsync(filterDef);
        var numberOfPages = (int)Math.Ceiling((double)total / perPage);

        var pipeline = new List<BsonDocument>();
        pipeline.Add(new BsonDocument("$match", filterDef));
        pipeline.Add(new BsonDocument("$sort", new BsonDocument("_id", -1)));
        pipeline.Add(new BsonDocument("$skip", (curentPage -1) * perPage));
        pipeline.Add(new BsonDocument("$limit", perPage));

        pipeline.AddRange(BuildPostWithCommentsAndUserPipeline());

        var posts = await _postCollection.Aggregate<PostResponse>(pipeline).ToListAsync();


        return new 
        {
            data = posts,
            numberOfPages,
            curentPage,
        };


}

public virtual async Task<Comment> CreateComment(string postId, string userId, string value)
    {
        var comment = new Comment
        {
            postId = postId,
            userId = userId,
            value = value,
            createdAt = DateTime.Now
        };

        await _commentCollection.InsertOneAsync(comment);
        return comment;

    }

    public async Task<bool> DeleteComment(string commentId, string userId, string postCreatorId)
    {
        var comment = await _commentCollection 
            .Find(x => x._id == commentId) 
            .FirstOrDefaultAsync(); 
        
        if (comment == null) return false; 
        // check authorization : user must be coment anthor or psot creator 
        if (comment.userId != userId && postCreatorId != userId)
        {
            throw new UnauthorizedAccessException("Not authorized to delte this comment");
        }

        var result = await _commentCollection.DeleteOneAsync(x => x._id == commentId);
        return result.DeletedCount > 0; 
    }

    public async Task<PostResponse?> GetPostWithComments(string postId)
    {
        return await GetPostByID(postId); // uses aggregation
    }

private List<BsonDocument> BuildPostWithCommentsAndUserPipeline(FilterDefinition<Post>? matchFilter = null)
{
    var pipeline = new List<BsonDocument>();
  
    // 1 match stage (if filter provided)
    if (matchFilter != null)
    {
        pipeline.Add(new BsonDocument("$match", matchFilter.Render( new RenderArgs<Post>(BsonSerializer.LookupSerializer<Post>(), BsonSerializer.SerializerRegistry))));
    }

    // 2 lookup user who created the post
    pipeline.Add(new BsonDocument("$lookup", new BsonDocument
    {
        { "from", "users" },
        { "localField", "creator" },
        { "foreignField", "_id" },
        { "as", "user" }
    }));

    // 3 unwind the user array
    pipeline.Add(new BsonDocument("$unwind", new BsonDocument
    {
        { "path", "$user" },
        { "preserveNullAndEmptyArrays", true }
    }));

    // 4 lookup comments (with nested user lookup )
    pipeline.Add(new BsonDocument("$lookup", new BsonDocument
    {
        { "from", "comments" },
        { "let", new BsonDocument("postId", "$_id") },
        { "pipeline", new BsonArray
            {
                // match comments for this post
                new BsonDocument("$match", new BsonDocument("$expr", 
                new BsonDocument("$eq", new BsonArray{"$postId", "$$postId"}))),

                // Sort comments by date (newest first)
                new BsonDocument("$sort", new BsonDocument("createdAt", -1)),

                // lookup user for each comment
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "let", new BsonDocument("uid", "$userId") },
                    { "pipeline", new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument("$expr", 
                            new BsonDocument("$eq", new BsonArray{"$_id", "$$uid"}))),
                            new BsonDocument("$project", new BsonDocument
                            {
                                { "_id", 0 },
                                { "name", 1 },
                                { "imageUrl", 1 }
                            })
                        }
                    },
                    { "as", "user" }
                }),
                // reunwind user obj
                new BsonDocument("$unwind", new BsonDocument
                        {
                            { "path", "$user" },
                            { "preserveNullAndEmptyArrays", true }
                }),
                // Project comment fields 
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "postId", 1 },
                    { "userId", 1 },
                    { "value", 1 },
                    { "createdAt", 1 },
                    { "user", 1}
                })
            }
        },
        { "as", "comments" }
    }));
  
   // 5. Final Projection  
   pipeline.Add(new BsonDocument("$project", new BsonDocument
   {
       { "_id", 1 },
       { "creator", 1 },
       { "title", 1 },
       { "message", 1 },
       {"selectedFile", 1 },
       {"likes", 1 },
       { "createdAt", 1 },
       { "comments", 1 },
       { "user.name", 1 },
       { "user.imageUrl", 1 }
   }));


   return pipeline;
    }
}