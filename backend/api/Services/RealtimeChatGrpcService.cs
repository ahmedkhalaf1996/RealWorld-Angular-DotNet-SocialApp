using Grpc.Core;
using backend.Protos;
using backend.Services;

namespace backend.Services
{
    public class RealtimeChatGrpcService : RealTimeChatService.RealTimeChatServiceBase
    {
        private readonly ChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<RealtimeChatGrpcService> _logger;

        public RealtimeChatGrpcService(ChatService chatService, IUserService userService , ILogger<RealtimeChatGrpcService> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        public override async Task<MessageResponse> SendMessage(MessageRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC Server : Recived messager form {request.Sender} to {request.Recever}");

            var msg = new backend.Models.Message
            {
                content = request.Content,
                sender = request.Sender,
                recever = request.Recever,
            };

            await _chatService.SendMessageAsync(msg, request.Sender, request.Recever);
            return new MessageResponse { Message = "Message Saved To DB" };
        }

        public override async Task<UsersIDsListResponse> GetUserFollowingFollowers(UserID request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC Server : Getting Followers/Following for user {request.Userid}");

            var response = new UsersIDsListResponse();

            try
            {
                var user = await _userService.GetUserByID(request.Userid); 
                if (user != null)
                {
                    var allUserIds = new List<string>();

                    if (user.followers != null)
                    {
                        allUserIds.AddRange(user.followers);
                    }

                    if (user.following != null)
                    {
                        allUserIds.AddRange(user.following);
                    }

                    allUserIds = allUserIds.Distinct().ToList();

                    var userIdsList = new UserIDsList();
                    userIdsList.UserIdsList.AddRange(allUserIds); 

                    response.UserIDsLists.Add(userIdsList);

                    _logger.LogInformation($"gRPC Server :returning {allUserIds.Count} user ids for {request.Userid}");
                } else
                {
                    _logger.LogInformation($"gRPC Server User {request.Userid} not found");
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"gRPC Server : Error gettting user relationships : {ex.Message}");                
            }

            return response;
        }
    }
}


