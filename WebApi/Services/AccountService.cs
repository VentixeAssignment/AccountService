using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Models;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AccountService(AccountRepository repository, DataContext context, ImageService imageService, ILogger<AccountService> logger, GrpcService grpcService, IHttpContextAccessor contextAccessor) : IAccountService
{
    private readonly AccountRepository _repository = repository;
    private readonly DataContext _context = context;
    private readonly ImageService _imageService = imageService;
    private readonly ILogger<AccountService> _logger = logger;
    private readonly GrpcService _grpcService = grpcService;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    public async Task<Result<AccountModel>> CreateAccountAsync(AccountRegForm form)
    {
        if (form == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Invalid data in registration form." };

        var exists = await _grpcService.AlreadyExistsAsync(form.Email);

        if (exists.Success)
            return new Result<AccountModel> { Succeeded = false, StatusCode = exists.StatusCode, Message = exists.Message };

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

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var reply = await _grpcService.CreateUserAsync(form.Email, form.Password);
            if (!reply.Success)
            {
                await transaction.RollbackAsync();
                return new Result<AccountModel> { Succeeded = false, StatusCode = reply.StatusCode, Message = reply.Message };
            }

            entity.UserId = reply.UserId;

            var result = await _repository.CreateAccountAsync(entity);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                try
                {
                    var deleteReply = await _grpcService.DeleteUserAsync(entity.UserId);

                    return deleteReply.Success
                        ? new Result<AccountModel> { Succeeded = true, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message }
                        : new Result<AccountModel> { Succeeded = false, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Something unexpected happened deleting user from Auth database. ##### {ex}");
                    return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting user from Auth database. ##### {ex}" };
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 201, Message = "Account successfully created." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened creating account. ##### {ex}");

            try
            {
                var deleteReply = await _grpcService.DeleteUserAsync(entity.UserId);

                return deleteReply.Success
                    ? new Result<AccountModel> { Succeeded = true, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message }
                    : new Result<AccountModel> { Succeeded = false, StatusCode = deleteReply.StatusCode, Message = deleteReply.Message };
            }
            catch (Exception exc)
            {
                _logger.LogWarning($"Something unexpected happened deleting user from Auth database. ##### {exc}");
                return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting user from Auth database. ##### {ex}" };
            }
        }
    }


    public async Task<Result<AccountModel>> GetOneAsync(Expression<Func<AccountEntity, bool>> expression)
    {
        if (expression == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Search term can not be empty." };

        var entity = await _repository.GetOneAsync(expression);

        if (entity == null || entity.Data == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = entity?.Message ?? "Account not found." };

        var emailReply = await _grpcService.GetUserEmailAsync(entity.Data.UserId);

        if (!emailReply.Success)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = emailReply.Message };

        var model = new AccountModel
        {
            Id = entity.Data.Id,
            UserId = entity.Data.UserId,
            FirstName = entity.Data.FirstName,
            LastName = entity.Data.LastName,
            PhoneNumber = entity.Data.PhoneNumber,
            Email = emailReply.Email,
            ProfileImageUrl = entity.Data.ProfileImageUrl,
            DateOfBirth = entity.Data.DateOfBirth,
            StreetAddress = entity.Data.StreetAddress,
            PostalCode = entity.Data.PostalCode,
            City = entity.Data.City
        };

        return new Result<AccountModel> { Succeeded = true, StatusCode = 200, Data = model };
    }


    public async Task<Result<AccountModel>> GetProfileInfoAsync()
    {
        var user = _contextAccessor.HttpContext?.User;
        if (user == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "No claims principal found in Http request." };

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(string.IsNullOrWhiteSpace(userId))
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = "No user found based on claim principals." };

        var entityResult = await _repository.GetOneAsync(x => x.UserId == userId);
        if (entityResult == null || entityResult.Data == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = $"No entity found with id {userId}." };

        var grpcReply = await _grpcService.GetUserEmailAsync(entityResult.Data.UserId);
        if (!grpcReply.Success || string.IsNullOrWhiteSpace(grpcReply.Email))
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = $"No email was found for user with id {entityResult.Data.UserId}." };

        var profile = new AccountModel
        {
            Id = entityResult.Data.Id,
            UserId = entityResult.Data.UserId,
            FirstName = entityResult.Data.FirstName,
            LastName = entityResult.Data.LastName,
            Email = grpcReply.Email,
            PhoneNumber = entityResult.Data.PhoneNumber,
            ProfileImageUrl = entityResult.Data.ProfileImageUrl,
            StreetAddress = entityResult.Data.StreetAddress,
            PostalCode = entityResult.Data.PostalCode,
            City = entityResult.Data.City,
            DateOfBirth = entityResult.Data.DateOfBirth
        };

        return new Result<AccountModel> { Succeeded = true, StatusCode = 200, Data = profile };
    }


    public async Task<Result<AccountModel>> UpdateAccountAsync(UpdateRegForm form)
    {
        if (form == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Input model is invalid." };

        var entity = await _repository.GetOneAsync(x => x.Id == form.Id);

        if (entity.Data == null)
            return new Result<AccountModel> { Succeeded = false, StatusCode = 404, Message = $"No entity with id {form.Id} was found." };


        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (form.ProfileImage != null)
            {
                entity.Data.ProfileImageUrl = _imageService.CreateImageUrl(form.ProfileImage!);
            }

            if (form.PhoneNumber != null)
            {
                entity.Data.PhoneNumber = form.PhoneNumber;
            }

            entity.Data.FirstName = form.FirstName;
            entity.Data.LastName = form.LastName;
            entity.Data.DateOfBirth = form.DateOfBirth;
            entity.Data.StreetAddress = form.StreetAddress;
            entity.Data.PostalCode = form.PostalCode;
            entity.Data.City = form.City;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 200, Message = "Account successfully updated." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened updating account. ##### {ex}");
            return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened updating account. ##### {ex}" };
        }
    }


    public async Task<Result<AccountModel>> DeleteAccountAsync(AccountModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Id) || string.IsNullOrWhiteSpace(model.UserId))
            return new Result<AccountModel> { Succeeded = false, StatusCode = 400, Message = "Invalid input data." };

        var entity = new AccountEntity
        {
            Id = model.Id
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reply = await _grpcService.ChangeActiveAsync(false, model.UserId);

            if (!reply.Success)
                return new Result<AccountModel> { Succeeded = false, StatusCode = reply.StatusCode, Message = reply.Message };

            var result = _repository.DeleteAccount(entity);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                try
                {
                    var statusResult = await _grpcService.ChangeActiveAsync(true, model.UserId);
                    if (!statusResult.Success)
                        return new Result<AccountModel> { Succeeded = false, StatusCode = reply.StatusCode, Message = reply.Message };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Something unexpected happened resetting user status. ##### {ex}");
                    return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened resetting user status. ##### {ex}" };
                }

                return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = result.Message };
            }

            await _context.SaveChangesAsync();

            try
            {
                var delete = await _grpcService.DeleteUserAsync(model.UserId);
                if (!delete.Success)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"Something unexpected happened deleting user. ##### {delete.StatusCode} ##### {delete.Message}");
                    return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting user. ##### {delete.StatusCode} ##### {delete.Message}" };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning($"Something unexpected happened deleting user. ##### {ex}");
                return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting user. ##### {ex}" };
            }

            await transaction.CommitAsync();

            return new Result<AccountModel> { Succeeded = true, StatusCode = 200, Message = "Account successfully deleted." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened deleting account. ##### {ex}");
            return new Result<AccountModel> { Succeeded = false, StatusCode = 500, Message = $"Something unexpected happened deleting account. ##### {ex}" };
        }
    }
}
