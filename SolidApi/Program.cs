using SolidApi.Database;
using SolidApi.Helpers;
using SolidApi.Repositories;
using SolidApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SolidDatabase>();

builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<SubscriptionRepository>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<DiscoveryService>();

// Exceções dos serviços -> 400 / 404 / 409 (ver SolidExceptionHandler)
builder.Services.AddExceptionHandler<SolidExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Cria as tabelas e o seed já no arranque, para um erro no SQL aparecer logo
app.Services.GetRequiredService<SolidDatabase>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
