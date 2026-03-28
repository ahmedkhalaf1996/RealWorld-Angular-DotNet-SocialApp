using realTimeServices.Dtos;
using realTimeServices.Services;
using Microsoft.AspNetCore.SignalR;

namespace realTimeServices.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        public ChatHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
           var UserID = Context.GetHttpContext()?.Request.Query["UserID"].ToString();
           if (UserID != "undefind" && UserID != "" && UserID is not null)
            {
                var GetAllRoomIdes = _chatService.AddAndGetUserRooms(UserID);
                foreach (var uid in GetAllRoomIdes)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, uid);
                }
                Console.WriteLine($"user connected id {UserID}");
                await Clients.Caller.SendAsync("UserConnected");
            }
            
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userid = _chatService.GetUserIdByConnectionID(Context.ConnectionId);
            if (userid is not null)
            {
                Console.WriteLine($"uid form conid {userid}");
                var uidlistfromRooms = _chatService.RemoveUserFromList(userid);
                if (uidlistfromRooms is not null)
                {
                    foreach (var uid in uidlistfromRooms)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, uid);
                        if (userid != uid)
                        {
                            await DisplayOnlineOtherUsers(uid);
                        }
                    }
                }
                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task AddUserConnectionId(string id)
        {
        if (id != "undefind" && id != "" && id is not null)
            {
                _chatService.AddUserConnectionId(id, Context.ConnectionId);
                await DisplayOnlineOtherUsers(id);
                await NotifyAllConnectedUsersToRefresh(id);
            }
        }

        private async Task NotifyAllConnectedUsersToRefresh(string newlyConnectedUserId)
        {
            if(newlyConnectedUserId != "" && newlyConnectedUserId != "undefind" && newlyConnectedUserId is not null)
            {
                var uidlistfromRooms = _chatService.GetOnlyUserRooms(newlyConnectedUserId);
                if(uidlistfromRooms != null)
                {
                    foreach(var uid in uidlistfromRooms)
                    {
                        await Clients.Group(uid).SendAsync("RefreshOnlineUsersNeeded", newlyConnectedUserId);
                    }
                }
            }
        }


        public async Task RefreshUserRooms(string userId)
        {
            if(userId != "" && userId != "undefind" && userId is not null)
            {
                var oldRooms = _chatService.GetOnlyUserRooms(userId);
                if(oldRooms != null)
                {
                    foreach(var room in oldRooms)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
                    }
                }

                var updatedRooms = _chatService.AddAndGetUserRooms(userId);

                // 
                foreach(var room in updatedRooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, room);
                }

                await NotifyAllConnectedUsersToRefresh(userId);
            }
        }

        public string[] GetOnlineUsersStatus(string userId)
        {
         if(userId != "" && userId != "undefind" && userId is not null)
            {
                var onlineUsers = _chatService.GetOnlineUsers(userId);
                if(onlineUsers != null)
                {
                    var filterdOnlineUsers = onlineUsers.Where(u => u != userId).ToArray();
                    return filterdOnlineUsers;
                }
            }
            return new string[] {};
        }
        private async Task DisplayOnlineOtherUsers(string id)
        {
            Console.WriteLine($"Display online called id {id}");
            if (id != "undefind" && id != "" && id is not null)
            {
                var uidlistfromRooms = _chatService.GetOnlyUserRooms(id);
                if (uidlistfromRooms is null) return;
                foreach (string uid in uidlistfromRooms)
                {
                    var onlineUsersx = _chatService.GetOnlineUsers(uid);
                    if (onlineUsersx != null)
                    {
                        var filterOnlineUsers = onlineUsersx.Where(u => u != uid).ToArray();
                        await Clients.Groups(uid).SendAsync("OnlineUsers" + uid, filterOnlineUsers);
                    }
                }
            }

        }


        public async Task RecivePrivateMessage(MessageDto message)
        {
            string privateGroupName = GetPrivateGroupName(message.sender, message.recever);
            await Groups.AddToGroupAsync(Context.ConnectionId, privateGroupName);
            var toConectionId = _chatService.GetConnectionIdByUser(message.recever);
            await Groups.AddToGroupAsync(toConectionId, privateGroupName);

            //
            await Clients.Client(toConectionId).SendAsync("OpenPrivateChat", message);
            await Clients.Group(privateGroupName).SendAsync("NewPrivateMessage", message);
            _chatService.SaveMessageToDb(message);
        }

        public async Task RemovePrivateChat(string sender, string recever)
        {
            string privateGroupName = GetPrivateGroupName(sender, recever);
            await Clients.Group(privateGroupName).SendAsync("ClosePrivateChat");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, privateGroupName);
            var toConnetionId = _chatService.GetConnectionIdByUser(recever);
            await Groups.RemoveFromGroupAsync(toConnetionId, privateGroupName);
            
        }
        private string GetPrivateGroupName(string sender, string recever)
        {
            var stringCompare = string.CompareOrdinal(sender, recever) < 0;
            return stringCompare ? $"{sender}-{recever}" : $"{recever}-{sender}";
        }

    }
}


