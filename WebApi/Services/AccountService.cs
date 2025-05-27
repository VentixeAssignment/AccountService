using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Models;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AccountService(AccountRepository repository, DataContext context, ImageService imageService, ILogger<AccountService> logger, AuthHandler.AuthHandlerClient authHandlerClient) : IAccountService
{
    private readonly AccountRepository _repository = repository;
    private readonly DataContext _context = context;
    private readonly ImageService _imageService = imageService;
    private readonly ILogger<AccountService> _logger = logger;
    private readonly AuthHandler.AuthHandlerClient _authHandlerClient = authHandlerClient;


    public async Task<Result<AccountModel>> CreateAccountAsync(AccountRegForm form)
    {
        if (form == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Invalid data in registration form." };

        var imageUrl = "";

        if (form.ProfileImage != null)
            imageUrl = _imageService.CreateImageUrl(form.ProfileImage);

        var entity = new AccountEntity
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            DateOfBirth = form.DateOfBirth,
            ProfileImageUrl = imageUrl,
            PhoneNumber = form.PhoneNumber,
            StreetAddress = form.StreetAddress,
            PostalCode = form.PostalCode,
            City = form.City
        };

        var request = new CreateRequest
        {
            Email = form.Email,
            Password = form.Password
        };
        
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var reply = await _authHandlerClient.CreateUserAsync(request);
            if(!reply.Success)
            {
                await transaction.RollbackAsync();
                return new Result<AccountModel> { Succeeded = false, StatusCode = reply.StatusCode, Message = reply.Message };
            }

            entity.UserId = reply.UserId;

            var result = await _repository.CreateAccountAsync(entity);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                var deleteRequest = new DeleteRequest
                {
                    Id = reply.UserId
                };

                try
                {
                    var deleteReply = await _authHandlerClient.DeleteUserAsync(deleteRequest);

                    return deleteReply.Success
                        ? new Result<AccountModel> { Succeeded = true, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message }
                        : new Result<AccountModel> { Succeeded = false, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Something unexpected happened deleting user from Auth database.\n{ex.Message}");
                    return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting user from Auth database.\n{ex.Message}" };
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 201, Message = "Account successfully created." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened creating account.\n{ex.Message}");
            return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened creating account.\n{ex.Message}" };
        }
    }


    public async Task<Result<AccountModel>> GetOneAsync(Expression<Func<AccountEntity, bool>> expression)
    {
        if (expression == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Search term can not be empty." };

        var entity = await _repository.GetOneAsync(expression);

        if (entity == null || entity.Data == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = entity?.Message ?? "Account not found." };

        var model = new AccountModel
        {
            Id = entity.Data.Id,
            FirstName = entity.Data.FirstName,
            LastName = entity.Data.LastName,
            ProfileImageUrl = entity.Data.ProfileImageUrl,
            DateOfBirth = entity.Data.DateOfBirth,
            StreetAddress = entity.Data.StreetAddress,
            PostalCode = entity.Data.PostalCode,
            City = entity.Data.City
        };

        return new Result<AccountModel> { Succeeded = true, StatusCode = 200, Data = model };
    }


    public async Task<Result<AccountModel>> UpdateAccountAsync(AccountModel model)
    {
        if (model == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Input model is invalid." };

        var entity = new AccountEntity
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            DateOfBirth = model.DateOfBirth,
            ProfileImageUrl = model.ProfileImageUrl,
            PhoneNumber = model.PhoneNumber,
            StreetAddress = model.StreetAddress,
            PostalCode = model.PostalCode,
            City = model.City
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = _repository.UpdateAccount(entity);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = result.Message };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 201, Message = "Account successfully updated." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened updating account.\n{ex.Message}");
            return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened updating account.\n{ex.Message}" };
        }
    }

    public async Task<Result<AccountModel>> DeleteAccountAsync(AccountModel model)
    {
        if (model == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Input model is invalid." };

        var entity = new AccountEntity
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            DateOfBirth = model.DateOfBirth,
            ProfileImageUrl = model.ProfileImageUrl,
            PhoneNumber = model.PhoneNumber,
            StreetAddress = model.StreetAddress,
            PostalCode = model.PostalCode,
            City = model.City
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = _repository.DeleteAccount(entity);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = result.Message };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 201, Message = "Account successfully deleted." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened deleting account.\n{ex.Message}");
            return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting account.\n{ex.Message}" };
        }
    }
}
