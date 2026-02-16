using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1️⃣ Add Services
// ===============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🔐 Swagger + JWT Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste JWT token. (Try with: Bearer <token>)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// ===============================
// 2️⃣ Database (✅ ثابت + واضح)
// ===============================
// نخزن DB في نفس مجلد المشروع (ContentRootPath)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
Console.WriteLine("✅ DB Path Used By App: " + dbPath);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ===============================
// 3️⃣ Custom Services
// ===============================
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt section is missing in appsettings.json");

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<NotesService>();

// ===============================
// 4️⃣ Authentication
// ===============================
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

var app = builder.Build();

// ===============================
// ✅ 5️⃣ Auto-Apply Migrations (الحل الجذري)
// ===============================
// هذا يضمن إنه نفس DB اللي التطبيق يستخدمها رح تنطبق عليها كل migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("✅ Applying migrations (if any)...");
    db.Database.Migrate();
    Console.WriteLine("✅ Database is ready.");
}

// ===============================
// 6️⃣ Middleware Pipeline
// ===============================
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
