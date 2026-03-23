using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Dapper;
using NetReactProjectBackEnd.Data;
using NetReactProjectBackEnd.Models;
using NetReactProjectBackEnd.Repositories;
using NetReactProjectBackEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("AllowReact",
    p => p.WithOrigins(builder.Configuration["Frontend:FEURL"]!)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));

var app = builder.Build();

// SEED ADMIN USER
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
    using var con = dbContext.CreateConnection();
    var existing = await con.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Username = 'admin'");
    if (existing == null)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        await con.ExecuteAsync("INSERT INTO Users (Username, PasswordHash) VALUES ('admin', @Hash)", new { Hash = hash });
    }
}

app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();