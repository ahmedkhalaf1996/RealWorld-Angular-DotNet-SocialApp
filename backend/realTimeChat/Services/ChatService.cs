using realTimeServices.Dtos;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using System.Text.Json;

namespace realTimeServices.Services
{
    public class ChatService
    {
        private readonly IDatabase _db;
        private readonly RealTimeChatClient _client;

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
            var client = new RealTimeChatClient();
            List<string> uides = new List<string>();

            if (userid != "undefined" && userid != "" && userid is not null)
            {
                var uidesx = _client.GetUsersIdes(userid);
                foreach( var id in uidesx)
                {
                   if(_db.HashExists("UserRooms", id))
                   {
                    if (!uides.Contains(id))
                    {
                        uides.Add(id);
                    }
                   }
                }

                _db.HashSet("UserRooms", userid, JsonSerializer.Serialize(uides));

                foreach (var uid in uides)
                {
                   var otherRoomJson = _db.HashGet("UserRooms", uid);
                   List<string> otherRoom = otherRoomJson.HasValue ? 
                   JsonSerializer.Deserialize<List<string>>(otherRoomJson.ToString()) ?? new List<string>()
                   : new List<string>();
                     if (!otherRoom.Contains(userid))
                     {
                      otherRoom.Add(userid);
                      _db.HashSet("UserRooms", uid, JsonSerializer.Serialize(otherRoom));
                     }
                }
                uides.Add(userid);
            }
            return uides;
        }

        public List<string>? RemoveUserFromList(string userid)
        {
            if (userid != "undefined" && userid != "" && userid is not null)
            {
                _db.HashDelete("UserRooms", userid);
                var roomsJson = _db.HashGet("UserRooms", userid);
                if (!roomsJson.HasValue) return null;

                var uides = JsonSerializer.Deserialize<List<string>>(roomsJson.ToString()) ?? new List<string>();
                var removedList = uides.ToList();

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
                removedList.Add(userid);
                return removedList;

            }
            return null;
        }

        public string[]? GetOnlineUsers(string id)
        {
            try
            {
                var online = GetOnlyUserRooms(id);
                return online?.ToArray() ?? new string[] {};
            }
            catch 
            {
                
               return null;
            }
        }

        public List<string>? GetOnlyUserRooms(string userid)
        {
            try
            {
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

        public void AddUserConnectionId(string userid, string connectionId)
        {
            if (userid != "undefined" && !string.IsNullOrEmpty(userid) && connectionId != "undefined" && !string.IsNullOrEmpty(connectionId))
            {
              _db.HashSet("OnlineUsers", userid, connectionId);
            }
        }

        // get user by connction id 
        public string GetUserIdByConnectionID(string connectionId)
        {
            if (connectionId != "undefined" && !string.IsNullOrEmpty(connectionId))
            {
                var onlineUsers = _db.HashGetAll("OnlineUsers");
                var user = onlineUsers.FirstOrDefault(x => x.Value == connectionId);
                return user.Name.HasValue ? user.Name.ToString() : "";
            }
            return "";
        }

        public string GetConnectionIdByUser(string user) // user represent userid
        {
            if (user != "undefined" && !string.IsNullOrEmpty(user))
            {
                var connectionId = _db.HashGet("OnlineUsers", user);
                return connectionId.HasValue ? connectionId.ToString() : "";
            }
            return "";
        }



    }
}