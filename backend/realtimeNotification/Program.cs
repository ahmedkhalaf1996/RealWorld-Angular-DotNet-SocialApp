using RealTimeNotification.Services;
using RealTimeNotification.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenAnyIP(81, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    
});;
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);

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

app.MapHub<NotificationHub>("/notificationHub");

app.Run();