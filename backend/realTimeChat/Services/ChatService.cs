
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using StackExchange.Redis;
using realTimeServices.Dtos;

namespace realTimeServices.Services
{
    public class ChatService
    {
        private readonly IDatabase _db;
        private readonly RealTimeChatClient _client;

        // IConnectionMultiplexer is injected natively so it connects to your k8s Redis container
        public ChatService(IConnectionMultiplexer redis, RealTimeChatClient client)
        {
            _db = redis.GetDatabase();
            _client = client;
        }

        public void SaveMessageToDb(MessageDto message)
        {
            _client.SendMessage(message.content, message.sender, message.recever);
        }

        public List<string> AddAndGetUserRooms(string userid)
        {
            List<string> uides = new List<string>();

            if (userid != "undefined" && !string.IsNullOrEmpty(userid))
            {
                var uidesx = _client.GetUsersIdes(userid);

                foreach (var id in uidesx)
                {
                    // Redis: Check if friend's room exists globally
                    if (_db.HashExists("UserRooms", id))
                    {
                        if (!uides.Contains(id)) uides.Add(id);
                    }
                }

                // Update this user's friends list globally in Redis
                _db.HashSet("UserRooms", userid, JsonSerializer.Serialize(uides));

                foreach (var uid in uides)
                {
                    // Pull friend's room from Redis
                    var otherRoomJson = _db.HashGet("UserRooms", uid);
                    List<string> otherRoom = otherRoomJson.HasValue
                        ? JsonSerializer.Deserialize<List<string>>(otherRoomJson.ToString()) ?? new List<string>()
                        : new List<string>();

                    if (!otherRoom.Contains(userid))
                    {
                        otherRoom.Add(userid);
                        // Save friend's updated room back to Redis
                        _db.HashSet("UserRooms", uid, JsonSerializer.Serialize(otherRoom));
                    }
                }

                uides.Add(userid);
            }

            return uides;
        }

        public List<string> RemoveUserFromList(string userid)
        {
            if (userid != "undefined" && !string.IsNullOrEmpty(userid))
            {
                // Globally remove connection ID 
                _db.HashDelete("OnlineUsers", userid);

                var roomsJson = _db.HashGet("UserRooms", userid);
                if (!roomsJson.HasValue) return null;

                var uides = JsonSerializer.Deserialize<List<string>>(roomsJson.ToString()) ?? new List<string>();
                var removedlist = uides.ToList();

                foreach (var uid in uides)
                {
                    var otherRoomJson = _db.HashGet("UserRooms", uid);
                    if (otherRoomJson.HasValue)
                    {
                        var otherRoom = JsonSerializer.Deserialize<List<string>>(otherRoomJson.ToString()) ?? new List<string>();
                        if (otherRoom.Remove(userid))
                        {
                            _db.HashSet("UserRooms", uid, JsonSerializer.Serialize(otherRoom));
                        }
                    }
                }

                // Globally remove user room
                _db.HashDelete("UserRooms", userid);
                removedlist.Add(userid);
                return removedlist;
            }
            return null;
        }

        public string[] GetOnlineUsers(string id)
        {
            try
            {
                var online = GetOnlyUserRooms(id);
                return online?.ToArray() ?? new string[] { };
            }
            catch
            {
                return null;
            }
        }

        public List<string> GetOnlyUserRooms(string userid)
        {
            try
            {
                // Read from global Redis cache instead of local Dictionary
                var roomsJson = _db.HashGet("UserRooms", userid);
                if (roomsJson.HasValue)
                {
                    return JsonSerializer.Deserialize<List<string>>(roomsJson.ToString()) ?? new List<string>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // connection method
        public void AddUserConnectionId(string userid, string connectionId)
        {
            if (userid != "undefined" && !string.IsNullOrEmpty(userid) &&
                connectionId != "undefined" && !string.IsNullOrEmpty(connectionId))
            {
                // Store connection ID globally in Redis Hash
                _db.HashSet("OnlineUsers", userid, connectionId);
            }
        }

        public string GetUserIdByConnectionID(string connectionId)
        {
            if (connectionId != "undefined" && !string.IsNullOrEmpty(connectionId))
            {
                // Retrieve all online users from Redis to find matching connectionId
                var onlineUsers = _db.HashGetAll("OnlineUsers");
                var user = onlineUsers.FirstOrDefault(x => x.Value == connectionId);
                return user.Key.HasValue ? user.Key.ToString() : "";
            }
            return "";
        }

        public string GetConnectionIdByUser(string user)
        {
            if (user != "undefined" && !string.IsNullOrEmpty(user))
            {
                // Fetch connection ID directly from Redis Hash
                var connId = _db.HashGet("OnlineUsers", user);
                return connId.HasValue ? connId.ToString() : "";
            }
            return "";
        }
    }
}


// using realTimeServices.Dtos;
// using System.Collections.Generic;
// using System.Linq;


// namespace realTimeServices.Services
// {
//     public class ChatService
//     {
//         private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();

//         public class Rooms
//         {
//             public Dictionary<string , List<String>> Room {get; set;} = new Dictionary<string, List<string>>();
//         }

//         Rooms room = new Rooms();

//         public void SaveMessageToDb(MessageDto message)
//         {
//             var client = new RealTimeChatClient();
//             client.SendMessage(message.content, message.sender, message.recever);
//         }

//         public List<string> AddAndGetUserRooms(string userid)
//         {
//             var client = new RealTimeChatClient();
//             List<string> uides = new List<string>();

//             if (userid != "undefined" && userid != "" && userid is not null)
//             {
//                 var uidesx = client.GetUsersIdes(userid);
//                 uidesx.ForEach((id) =>
//                 {
//                   if (room.Room.ContainsKey(id))
//                     {
//                         //uides.Add(id);
//                         if (!uides.Contains(id)) uides.Add(id);
//                     }  
//                 });

//                 if (room.Room.ContainsKey(userid))
//                 {
//                     room.Room[userid] = uides;
//                 } else
//                 {
//                     room.Room.Add(userid, uides);
//                 }

//                 // add userid to other users rooms
//                 foreach(var uid in uides)
//                 {
//                     if (room.Room.ContainsKey(uid))
//                     {
//                         // room.Room[uid].Add(userid);
//                         if (!room.Room[uid].Contains(userid)) room.Room[uid].Add(userid);
//                     }
//                 }

//                 uides.Add(userid);
//             }
//             return uides;
//         }

//         public List<string>? RemoveUserFromList(string userid)
//         {
//             if (userid != "undefined" && userid != "" && userid is not null)
//             {
//                 lock (Users)
//                 {
//                     if (Users.ContainsKey(userid))
//                     {
//                         Users.Remove(userid);
//                     }
//                 }

//                 var uides = room.Room[userid];
//                 var removedlist = room.Room[userid];

//                 foreach (var uid in uides.ToArray())
//                 {
//                     if (room.Room.ContainsKey(uid))
//                     {
//                         room.Room[uid].Remove(userid);
//                     }
//                 }

//                 if (room.Room.ContainsKey(userid))
//                 {
//                     room.Room.Remove(userid);
//                 }

//                 removedlist.Add(userid);
//                 return removedlist;
//             } else
//             {
//                 return null;
//             }

//         }

//         public string[]? GetOnlineUsers(string id)
//         {
//             try
//             {
//                 var online = room.Room[id].ToArray();
//                 return online;
//             }
//             catch 
//             {
                
//                return null;
//             }
//         }

//         public List<string>? GetOnlyUserRooms(string userid)
//         {
//             try
//             {
//                 List<string> uidesList = room.Room[userid];
//                 return uidesList;
//             }
//             catch 
//             {
                
//               return null;
//             }
//         }

//         public void AddUserConnectionId(string userid, string connectionId)
//         {
//             if (
//                 userid != "undefined" &&
//                 userid != "" && 
//                 userid is not null &&
//                 connectionId != "undefined" &&
//                 connectionId != "" && 
//                 connectionId is not null 
//                 )
//             {
//                 lock(Users)
//                 {
//                     if (!Users.ContainsKey(userid))
//                     {
//                         Users[userid] = connectionId;
//                     }
//                 }
//             }
//         }

//         // get user by connction id 
//         public string GetUserIdByConnectionID(string connectionId)
//         {
//             if (  connectionId != "undefined" &&  connectionId != "" &&  connectionId is not null )
//             {
//                 lock(Users)
//                 {
//                     var list = Users.Where(x => x.Value == connectionId).Select(x => x.Key).FirstOrDefault();
//                     if (list is not null)
//                     {
//                         return list;
//                     } else
//                     {
//                         return "";
//                     }
//                 }
//             } else
//             {
//                 return "";
//             }
//         }

//         public string GetConnectionIdByUser(string user) // user represent userid
//         {
//             if(user != "undefined" && user != "")
//             {
//                 lock (Users)
//                 {
//                     var list = Users.Where(x => x.Key == user).Select(x => x.Value).FirstOrDefault();
//                     if (list is not null)
//                     {
//                         return list;
//                     } else
//                     {
//                         return "";
//                     }
//                 }
//             } else
//             {
//                 return "";
//             }
//         }



//     }
// }