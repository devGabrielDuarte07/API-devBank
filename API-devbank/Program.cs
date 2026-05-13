using API_devbank.Models;
using API_devbank.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// RENDER PORT
// ==========================================

builder.WebHost.UseUrls("http://0.0.0.0:10000");

// ==========================================
// CONTROLLERS
// ==========================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// ==========================================
// CORS
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevBankPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==========================================
// SWAGGER
// ==========================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevBank API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer SEU_TOKEN"
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

// ==========================================
// DATABASE
// ==========================================

var connectionString =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection"
    );


Console.WriteLine(connectionString);

builder.Services.AddDbContext<DevbankContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure();
        }
    );
});

// ==========================================
// SERVICES
// ==========================================

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PixService>();

// ==========================================
// JWT
// ==========================================

var jwtKey =
    Environment.GetEnvironmentVariable("Jwt__Key");

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key não configurada.");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key),

                ValidateIssuer = false,

                ValidateAudience = false,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

// ==========================================
// BUILD
// ==========================================

var app = builder.Build();

// ==========================================
// PIPELINE
// ==========================================

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("DevBankPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();