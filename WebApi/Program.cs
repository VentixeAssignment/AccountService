using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Protos;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<AccountHandler.AccountHandlerClient>(x => 
    x.Address = new Uri("https://localhost:7177")
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));
builder.Services.AddScoped<IAuthService, IAuthService>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(x =>
{
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
    x.Password.RequireDigit = true;
    x.Password.RequireUppercase = true;
    x.Password.RequireLowercase = true;
}).AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
