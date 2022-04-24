using Basic.Data;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services
    .AddSapphireDb()
    .AddContext<BasicDbContext>(cfg => cfg.UseInMemoryDatabase("basic"));

var app = builder.Build();

app.UseCors(cfg => cfg.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(origin => true).AllowCredentials());

app.UseSapphireDb();

app.MapGet("/", () => "Hello World!");

app.Run();