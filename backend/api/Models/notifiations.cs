using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Models
{
    public class Notification {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id {get;set;} = null!;
        public string deatils {get;set;} = null!;
        public string mainuid {get;set;} = null!;
        public string targetid {get;set;} = null!;

        public bool isreded {get; set;} = false;
        public DateTime? createdAt {get; set;} = DateTime.Now;

        public UserIn user {get;set;} = null!;
    }

    public class UserIn {
        public string name {get;set;} = null!;
        public string avatar {get;set;} = null!;

    }
}





