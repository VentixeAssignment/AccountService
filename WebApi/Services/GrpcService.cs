using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using WebApi.Protos;
using static Grpc.Core.Metadata;
using static WebApi.Protos.AuthHandler;

namespace WebApi.Services;

public class GrpcService
{
    private readonly IConfiguration _config;
    private readonly ILogger<GrpcService> _logger ;
    private readonly AuthHandler.AuthHandlerClient _client;

    public GrpcService(IConfiguration config, ILogger<GrpcService> logger)
    {
        _logger = logger;
        _config = config; var uri = _config["GrpcUri"];
        logger.LogInformation("Using GrpcUri: {Uri}", uri);
        var channel = GrpcChannel.ForAddress(_config["GrpcUri"]);
        _client = new AuthHandler.AuthHandlerClient(channel);
        _logger = logger;

        Console.WriteLine($"gRPC channel state: {channel.State}");
    }

    public async Task<ExistsReply> AlreadyExistsAsync(string email)
    {
        var request = new ExistsRequest { Email = email };

        var reply = await _client.AlreadyExistsAsync(request);
        return reply;
    }


    public async Task<CreateReply> CreateUserAsync(string email, string password)
    {
        var request = new CreateRequest
        {
            Email = email,
            Password = password
        };

        var reply = await _client.CreateUserAsync(request);
        return reply;
    }



    public async Task<EmailReply> GetUserEmailAsync(string id)
    {
        var request = new EmailRequest
        {
            Id = id
        };
        var reply = await _client.GetUserEmailAsync(request);
        return reply;
    }


    public async Task<ActiveReply> ChangeActiveAsync(bool active, string id)
    {
        var request = new ActiveRequest
        {
            IsActive = active,
            Id = id
        };
        var reply = await _client.ChangeActiveAsync(request);
        return reply;
    }

    public async Task<DeleteReply> DeleteUserAsync(string id)
    {
        var deleteRequest = new DeleteRequest
        {
            Id = id
        };

        var deleteReply = await _client.DeleteUserAsync(deleteRequest);
        return deleteReply;
    }  
}
