using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme); //todo: switch to jwt bearer tokens

string? dbConnectionString = builder.Configuration.GetConnectionString("SqlServerDatabase");

builder.Services.AddDbContext<TimesynqDbContext>(options => options.UseSqlServer(dbConnectionString));

builder.Services.AddIdentityCore<TimesynqUser>()
    .AddEntityFrameworkStores<TimesynqDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<TimesynqUser>();


app.Run();
