using System.Reflection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

#region Add services to the container

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Redis Features API",
        Description = "Taking a test drive of the various Redis datatype utilizing C#"
    });

    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});
builder.Services
    .AddStackExchangeRedisExtensions<NewtonsoftSerializer>(builder.Configuration.GetSection("RedisConfig")
    .Get<RedisConfiguration>());

#endregion

#region Configure the HTTP request pipeline
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(_ =>
    _.SwaggerEndpoint("/swagger/v1/swagger.json", "Redis Features API v1"));

app.UseCors(policy =>
    policy.WithOrigins("http://localhost:5068")
        .AllowAnyMethod()
        .WithHeaders(HeaderNames.ContentType));

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
