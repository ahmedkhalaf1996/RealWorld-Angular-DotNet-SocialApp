using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]

    public string? _id {get; set;}

    public string name {get; set;} = null!;
    public string email {get; set;} = null!;
    public string password {get; set;} = null!;
    public string imageUrl {get; set;} = null!;
    public string bio {get; set;} = null!;

    public List<string> followers {get; set;} = new List<string>{};
    public List<string> following {get; set;} = new List<string>{};

    internal string DecryptPasswordBase64(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    internal string EncryptPasswordBase64(string text)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
        return System.Convert.ToBase64String(plainTextBytes);
    }


}