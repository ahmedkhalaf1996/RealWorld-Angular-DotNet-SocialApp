using RealTimeNotification.Services;
using RealTimeNotification.Hubs;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddControllers(); 
// builder.Services.AddEndpointsApiExplorer(); 

builder.Services.AddSignalR();
builder.Services.AddCors();
// grpc 
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();


var app = builder.Build();


app.UseCors(builder => builder 
    .AllowAnyHeader() 
    .AllowAnyMethod()
    .AllowCredentials() 
    .SetIsOriginAllowed(_ => true) /// allwo any origin for tesitg
);

// gpr 
app.MapGrpcService<NotificationService>();
app.MapGrpcReflectionService();


// app.UseAuthorization();
// app.MapControllers();

app.MapHub<NotificationHub>("/hubs/Notifications");

app.Run();