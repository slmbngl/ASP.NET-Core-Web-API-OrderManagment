using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;

var builder = WebApplication.CreateBuilder(args);

// **********************************************
// EF Core ve MSSQL Hizmetini Ekleme
// **********************************************
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// **********************************************

// Diğer servisler (Controller'lar, Swagger vb.)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Swagger'ı etkinleştirme (API dokümantasyonu)
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();