using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters; // <<< YENİ EKLENEN USING
var builder = WebApplication.CreateBuilder(args);


// **********************************************
// 2. JWT DOĞRULAMAYI EKLEME (Authentication)
// **********************************************
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret not found.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // **********************************************
    // YENİ EKLENEN/GÜNCELLENEN PAROLA AYARLARI
    // **********************************************
    options.Password.RequireDigit = false;            // Rakam (0-9) gereksinimini kaldır
    options.Password.RequireLowercase = false;        // Küçük harf gereksinimini kaldır (isteğe bağlı)
    options.Password.RequireUppercase = false;        // Büyük harf (A-Z) gereksinimini kaldır
    options.Password.RequireNonAlphanumeric = false;  // Sembol (!, @, # vb.) gereksinimini kaldır
    options.Password.RequiredLength = 6;              // Minimum karakter sayısını belirle (6 önerilir)
    options.Password.RequiredUniqueChars = 1;         // Tekrarsız karakter sayısını belirle
    // **********************************************
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// **********************************************
// EF Core ve MSSQL Hizmetini Ekleme
// **********************************************
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// **********************************************
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
// Diğer servisler (Controller'lar, Swagger vb.)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Management API", Version = "v1" });

    // **********************************************
    // YENİ EKLEME: AUTHORIZE BUTONU VE AYARLARI
    // **********************************************
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme. Örn: 'Bearer {token}'",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer" // Küçük harfle 'Bearer'
});

// 2. [Authorize] Etiketini Gördüğünde Token Gereksinimi Ekleme
option.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] { } // Boş string dizisi, tüm scope'lar için geçerli demektir.
    }
});
    // 3. OperationFilter'ı Ekleyin (Korunan endpoint'lere kilit simgesi koymak için)
    option.OperationFilter<SecurityRequirementsOperationFilter>();

    // 4. (Opsiyonel ama önerilir) Endpoint'lerin hangi Policy ile korunduğunu gösterir
    option.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();    // **********************************************
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Swagger'ı etkinleştirme (API dokümantasyonu)
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();