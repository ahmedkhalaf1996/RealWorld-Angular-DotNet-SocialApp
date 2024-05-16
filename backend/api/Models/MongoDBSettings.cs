namespace backend.Models;

public class MongoDBSettings {
    public string ConnectionString {get; set;} = null!;
    public string DatabaseName {get; set;} = null!;
    public string PostCollection {get; set;} = null!;
    public string UserCollection {get; set;} = null!;
    public string MessageCollection {get; set;} = null!;
    public string UnMessageCollection {get; set;} = null!;

    public string NotificationCollection {get; set;} = null!;

}