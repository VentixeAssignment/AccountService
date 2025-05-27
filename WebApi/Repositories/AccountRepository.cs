using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Repositories;

public class AccountRepository(DataContext context, ILogger<AccountRepository> logger)
{
    protected readonly DataContext _context = context;
    protected readonly DbSet<AccountEntity> _dbSet = context.Set<AccountEntity>();
    private readonly ILogger<AccountRepository> _logger = logger;


    public async Task<Result<AccountEntity>> CreateAccountAsync(AccountEntity entity)
    {
        if (entity == null)
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 400, Message = "Invalid data in registration form." };

        try
        {
            var result = await _context.AddAsync(entity);

            return result != null
                ? new Result<AccountEntity> { Succeeded = true, StatusCode = 201, Data = entity }
                : new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = "Failed to create account." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something unexpected happened adding account to database.\n{ex.Message}");
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened adding account to database.\n{ex.Message}" };
        }
    }


    public async Task<Result<AccountEntity>> GetOneAsync(Expression<Func<AccountEntity, bool>> expression)
    {
        if (expression == null)
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 400, Message = "Invalid expression." };

        var entity = await _dbSet.FirstOrDefaultAsync(expression);

        return entity != null
            ? new Result<AccountEntity> { Succeeded = true, StatusCode = 200, Data = entity }
            : new Result<AccountEntity> { Succeeded = false, StatusCode = 404, Message = $"No account found matching expression: {expression}" };
    }


    public Result<AccountEntity> UpdateAccount(AccountEntity entity)
    {
        if(entity == null)
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 400, Message = "Invalid entity data." };

        try
        {
            var result = _dbSet.Update(entity);

            return result != null
                ? new Result<AccountEntity> { Succeeded = true, StatusCode = 200, Data = entity }
                : new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = "Failed to update account." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something unexpected happened updating account in database.\n{ex.Message}");
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened updating account in database.\n{ex.Message}" };
        }
    }


    public Result<AccountEntity> DeleteAccount(AccountEntity entity)
    {
        if (entity == null)
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 400, Message = "Invalid entity data." };

        try
        {
            var result = _dbSet.Remove(entity);

            return result != null
                ? new Result<AccountEntity> { Succeeded = true, StatusCode = 200, Data = entity }
                : new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = "Failed to delete account." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something unexpected happened deleting account from database.\n{ex.Message}");
            return new Result<AccountEntity> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting account from database.\n{ex.Message}" };
        }
    }
}
