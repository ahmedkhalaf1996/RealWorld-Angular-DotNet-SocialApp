using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models;


public class UnReadedMessages {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id {get;set;}
    public string mainUserid {get; set; } = null!;
    public string otherUserid {get; set; } = null!;
    public bool isReaded {get; set; } = false;
    public int numOfUnreadedMessages {get; set; } = 0;
    
}

