using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models;

// 1. base comment model (what's stored in mongoDB)
public class Comment {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id {get; set;}

    [BsonElement("postId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? postId {get; set;}

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? userId {get; set;}

    [BsonElement("value")]
    public string? value {get; set;}

    [BsonElement("createdAt")]
    public DateTime? createdAt {get; set;} = DateTime.Now;
}
// 2 comment with user deatils (for api response after aggregation)

public class CommentWithUser {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id {get; set;}

    [BsonElement("postId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? postId {get; set;}

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? userId {get; set;}

    [BsonElement("value")]
    public string? value {get; set;}


    [BsonElement("createdAt")]
    public DateTime? createdAt {get; set;} = DateTime.Now;

    public UsserInfo? user {get; set;}
}

// 2. user infor subset for comemnt response
[BsonIgnoreExtraElements]
public class UsserInfo {
    public string? name {get; set;}
    public string? imageUrl {get; set;}
}

public class CreateCommentInterface {
       public string? value {get; set;}
 
}