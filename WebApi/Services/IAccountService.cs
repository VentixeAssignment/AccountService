using EmailFunction.Models;
using System.Linq.Expressions;
using WebApi.Dtos;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IAccountService
    {
        Task<Result<VerifyAccountRegForm>> SendVerificationEmailAsync(string email);
        Task<Result<VerifyVerificationCodeModel>> VerifyVerificationCode(VerifyAccountRegForm model);
        Task<Result<AccountModel>> CreateAccountAsync(AccountRegForm form);
        Task<Result<AccountModel>> GetOneAsync(Expression<Func<AccountEntity, bool>> expression);
        Task<Result<AccountModel>> GetProfileInfoAsync();
        Task<Result<AccountModel>> UpdateAccountAsync(UpdateRegForm form);
        Task<Result<AccountModel>> DeleteAccountAsync(AccountModel model);
    }
}