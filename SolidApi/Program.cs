using SolidApi.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SolidDatabase>();

var app = builder.Build();

// Cria as tabelas e o seed já no arranque, para um erro no SQL aparecer logo
app.Services.GetRequiredService<SolidDatabase>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();