using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models;


public class Message {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id {get;set;}
    public string content {get; set; } = null!;
    public string sender {get; set; } = null!;
    public string recever {get; set; } = null!;
    
}


