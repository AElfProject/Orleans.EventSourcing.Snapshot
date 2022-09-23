using System.Configuration;
using Orleans.EventSourcing.API.Contracts;
using Orleans.EventSourcing.API.Service;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddTransient<IKvConnection, KvConnectionListHelper>();
//builder.Services.AddTransient<IKvConnection, KvConnectionAllHelper>();
builder.Services.AddTransient<IKvConnection, KvConnectionPartitionHelper>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();