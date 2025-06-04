using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using WebApi.Data;
using WebApi.Protos;
using WebApi.Repositories;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddGrpcClient<AuthHandler.AuthHandlerClient>(x =>
    x.Address = new Uri("https://localhost:7177")
);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["Jwt:Issuer"],
        ValidateIssuer = true,
        ValidAudience = config["Jwt:Audience"],
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDb")));
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOpenApi();
app.MapScalarApiReference("/api/docs");
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
