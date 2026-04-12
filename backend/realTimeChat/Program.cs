using realTimeServices.Services;
using realTimeServices.Hubs;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenAnyIP(81, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    
});;

builder.Services.AddSingleton<ChatService>();

// redis 
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection)
);


builder.Services.AddCors();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true)); // allow any ogiring for testing 

// app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chatHub");

app.Run();

