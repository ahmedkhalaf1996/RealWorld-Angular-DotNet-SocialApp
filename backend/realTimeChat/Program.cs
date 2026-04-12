using realTimeServices.Services;
using realTimeServices.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenAnyIP(81, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    
});

// redis configuration
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

// Adds SignalR Redis Message Backplane Support
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);

// Explicitly register Redis Connection Multiplexer to inject into ChatService for multi-pod Shared Memory
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection)
);

// Register Custom Services for Dependency Injection
builder.Services.AddSingleton<RealTimeChatClient>();
builder.Services.AddSingleton<ChatService>();

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



// using realTimeServices.Services;
// using realTimeServices.Hubs;
// var builder = WebApplication.CreateBuilder(args);
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(80, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
//     options.ListenAnyIP(81, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    
// });;

// builder.Services.AddSingleton<ChatService>();

// // redis 
// var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

// builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);
// builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
//     StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection)
// );


// builder.Services.AddCors();
// builder.Services.AddGrpc();

// var app = builder.Build();

// app.UseCors(builder => builder
//     .AllowAnyHeader()
//     .AllowAnyMethod()
//     .AllowCredentials()
//     .SetIsOriginAllowed(_ => true)); // allow any ogiring for testing 

// // app.UseHttpsRedirection();

// app.MapHub<ChatHub>("/chatHub");

// app.Run();

