using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext(options)
{
}
