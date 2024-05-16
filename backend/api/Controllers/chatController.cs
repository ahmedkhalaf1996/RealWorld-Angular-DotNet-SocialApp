using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using backend.Services; 
using backend.interfaces;
using backend.Models;

namespace backend.Conrollers;

[Controller]
[Route("/chat")]

public class ChatController: Controller {
    private readonly IConfiguration _configuration;
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService, IConfiguration configuration){
        _chatService = chatService;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("sendmessage")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageInterface body){
        if(body.content ==null|| body.sender ==null|| body.recever ==null){
                        return BadRequest();

        }
        var msg = new Message{};
        msg.content = body.content;
        msg.sender = body.sender;
        msg.recever = body.recever;
 
        await _chatService.SendMessageAsync(msg, body.sender, body.recever);
        if(msg == null){
            return BadRequest();
        }
        return Ok(new {sucuess = true});

    }

    [HttpGet]
    [Route("getmsgsbynums")]
    public async Task<IActionResult> GetMessagesByNumsBetwenTwoUsers([FromQuery] string from, [FromQuery] string firstuid, [FromQuery] string seconduid){
        if(string.IsNullOrEmpty(from)|| string.IsNullOrEmpty(firstuid) || string.IsNullOrEmpty(seconduid)){
            return BadRequest(new {message = "problem with provided query parameters."});
        }

        List<Message> msgs = await _chatService.GetMessageByNum(int.Parse(from), firstuid, seconduid);
        return Ok(new { msgs });
    }

    [HttpGet]
    [Route("get-user-unreadedmsg")]
    public async Task<IActionResult> GetUserUnREadedMessage([FromQuery] string userid){
        if(string.IsNullOrEmpty(userid)){
             return BadRequest(new {message = "problem with provided query parameters."});
        }

        List<UnReadedMessages> urm = await _chatService.GetUserUnreadedmsgs(userid);

        int totalUnreadedMessageCount = urm.Sum(msg => msg.numOfUnreadedMessages);

        return Ok(new {messages = urm, total = totalUnreadedMessageCount});
    }


    
    [HttpGet]
    [Route("mark-msg-asreaded")]
    public async Task<IActionResult> MarkMessageAsReaded([FromQuery] string mainuid, [FromQuery] string otheruid){ 
        if(string.IsNullOrEmpty(mainuid) || string.IsNullOrEmpty(otheruid)){
             return BadRequest(new {message = "problem with provided query parameters."});
        }

        bool isMarked = await _chatService.MarkMsgsAsReaded(otheruid, mainuid);
        return Ok(new {isMarked});
    }

}