using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models;
using backend.interfaces;

namespace backend.Conrollers;

[Controller]
[Route("/notification")]

public class NotificationController: Controller {
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService){
        _notificationService = notificationService;
    }


    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetUserNotification([FromRoute] string id){
        if (id is null){
             return BadRequest(new {message = "proplem with proivded id"});
        }

        List<Notification> notifications = await _notificationService.GetUserNotification(id);
        
        if (notifications is null) return NotFound(new {message = "No notifications yes", Success = false});
        return Ok(new {notifications});
    }

    [HttpGet]
    [Route("mark-notification-asreaded")]
    public async Task<IActionResult> MarkNofificationAsReaded([FromQuery] string id){
        if(string.IsNullOrEmpty(id)){
            return BadRequest(new {message = "proplem with proivded id"});
        }

        bool isMarked = await _notificationService.MarkNotificationsAsReaded(id);
        if(!isMarked){
          return BadRequest(new {message = "proplem can not mark tnotification as readed."});

        }
        List<Notification> notifications = await _notificationService.GetUserNotification(id);
        
        if (notifications is null) return NotFound(new {message = "No notifications yes", Success = false});
        return Ok(new {notifications});
        
     }
}



