using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Protos;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<AuthHandler.AuthHandlerClient>(x =>
    x.Address = new Uri("https://localhost:7177")
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("LocalDb")));
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
