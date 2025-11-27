using Cartera_Cripto.Data;
using Cartera_Cripto.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDbContext<AppDBcontext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("PermitirTodo");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDBcontext>();

    if (!db.Monedas.Any())
    {
        db.Monedas.AddRange(
            new Moneda { Nombre = "Bitcoin", Abreviatura = "btc" },
            new Moneda { Nombre = "Ethereum", Abreviatura = "eth" },
            new Moneda { Nombre = "USD Coin", Abreviatura = "usdc" }
        );

        db.SaveChanges();
    }
}

app.Run();
