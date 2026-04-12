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

        // Automatically injected by Program.cs
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
                    // If the friend is active in Redis
                    if (_db.HashExists("UserRooms", id))
                    {
                        if (!uides.Contains(id)) uides.Add(id);
                    }
                }

                // Update this user's friends list in Redis
                _db.HashSet("UserRooms", userid, JsonSerializer.Serialize(uides));

                // Add this user to all their friends' rooms in Redis
                foreach (var uid in uides)
                {
                    var otherRoomJson = _db.HashGet("UserRooms", uid);
                    List<string> otherRoom = otherRoomJson.HasValue
                        ? JsonSerializer.Deserialize<List<string>>(otherRoomJson.ToString()) ?? new List<string>()
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

        public List<string> RemoveUserFromList(string userid)
        {
            if (userid != "undefined" && !string.IsNullOrEmpty(userid))
            {
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
                return online?.ToArray();
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
                _db.HashSet("OnlineUsers", userid, connectionId);
            }
        }

        // get user by connection id
        public string GetUserIdByConnectionId(string connectionId)
        {
            if (connectionId != "undefined" && !string.IsNullOrEmpty(connectionId))
            {
                var onlineUsers = _db.HashGetAll("OnlineUsers");
                var user = onlineUsers.FirstOrDefault(x => x.Value == connectionId);
                return user.Key.HasValue ? user.Key.ToString() : "";
            }
            return "";
        }

        // get connection id by user
        public string GetConnectionIdByUser(string user)
        {
            if (user != "undefined" && !string.IsNullOrEmpty(user))
            {
                var connId = _db.HashGet("OnlineUsers", user);
                return connId.HasValue ? connId.ToString() : "";
            }
            return "";
        }
    }
}
