using Basic.Data;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSapphireDb()
    .AddContext<BasicDbContext>(cfg => cfg.UseInMemoryDatabase("basic"));

var app = builder.Build();

app.UseSapphireDb();

app.MapGet("/", () => "Hello World!");

app.Run();