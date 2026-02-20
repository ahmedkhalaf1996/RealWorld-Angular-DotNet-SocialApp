using realTimeServices.Services;
using realTimeServices.Hubs;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ChatService>();

builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true)); // allow any ogiring for testing 

// app.UseHttpsRedirection();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();

