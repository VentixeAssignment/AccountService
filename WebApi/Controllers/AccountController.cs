using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;


[Route("api/account")]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;


    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetOneAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("No valid search term given.");

        var account = await _accountService.GetOneAsync(x => x.Id == id);

        return account.Succeeded
            ? Ok(account.Data)
            : NotFound(new { account.Message });
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateAsync([FromBody] AccountRegForm form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.CreateAccountAsync(form);

        if (result.StatusCode == 409)
            return Conflict(new {result.Message});

        if(result.StatusCode == 400)
            return BadRequest(new {result.Message});

        return result.Succeeded
            ? Created(string.Empty, new { result.Message })
            : StatusCode(result.StatusCode, new { result.Message });            
    }

    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> UpdateAsync(UpdateRegForm form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.UpdateAccountAsync(form);

        if (result.StatusCode == 400)
            return BadRequest(new { result.Message });

        if (result.StatusCode == 404)
            return NotFound(new { result.Message });

        return result.Succeeded
            ? Ok(new { result.Message })
            : StatusCode(result.StatusCode, new { result.Message });
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteAsync(AccountModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.DeleteAccountAsync(model);

        if(result.StatusCode == 400)
            return BadRequest(new { result.Message });

        if (result.StatusCode == 404)
            return NotFound(new { result.Message });

        return result.Succeeded
            ? Ok(new { result.Message })
            : StatusCode(result.StatusCode, new {result.Message});
    }
}
