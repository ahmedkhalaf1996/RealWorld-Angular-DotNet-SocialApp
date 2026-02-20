using realTimeServices.Protos;
using Grpc.Net.Client;


public class RealTimeChatClient
{
    private readonly RealTimeChatService.RealTimeChatServiceClient _client;

    public RealTimeChatClient()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5001");
        _client = new RealTimeChatService.RealTimeChatServiceClient(channel);
    }


    public void SendMessage(string content, string sender, string recever)
    {
        var request = new MessageRequest
        {
            Content = content,
            Sender = sender,
            Recever = recever
        };

        try
        {
            var response = _client.SendMessage(request);
            Console.WriteLine($"Received message: {response.Message}");
        }
        catch (Exception ex)
        {
         Console.WriteLine($"Warining : could not send message to amin api via grpc : {ex.Message}");
            
        }
    }


    // get user frinds using grpc form api
    public List<string> GetUsersIdes(string id)
    {
        var request = new UserID
        {
            Userid = id
        };
        if (request.Userid != "undefind" && request.Userid != "")
        {
            try
            {
                var response = _client.GetUserFollowingFollowers(request);
                var userIDsList = response.UserIDsLists.FirstOrDefault();
                if (userIDsList != null )
                {
                    return userIDsList.UserIdsList.ToList();
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Warining : could not conntxt to amin api for user reslatjionships : {ex.Message}");
            }
        }
        return new List<string>();
    }


}