using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models;


public class Post {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id {get; set;}
    [BsonElement("title")]
    public string? title {get; set;}

    [BsonRepresentation(BsonType.ObjectId)]
    public string? creator {get; set;}
    [BsonElement("message")]
    public string? message {get; set;}
    public string? selectedFile {get; set;}

    public List<string> likes {get; set;} = new List<string>{};

    [BsonIgnore] // don't store in mongoDB
    public List<CommentWithUser>? comments {get; set;} 

    public DateTime? createdAt {get; set;} = DateTime.Now;

    [BsonIgnore]
    public UsserInfo? user {get; set;}

}







