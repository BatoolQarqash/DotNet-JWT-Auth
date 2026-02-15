using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

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
        Description = "Paste ONLY the JWT token (without 'Bearer ')"

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
            new string[] {}
        }
    });
});

// ===============================
// 2️⃣ Database
// ===============================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// ===============================
// 3️⃣ Custom Services
// ===============================

builder.Services.AddScoped<PasswordService>();

// JWT Settings
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();
  


builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<JwtService>();

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
// 5️⃣ Middleware Pipeline
// ===============================

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();   // لازم قبل Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
