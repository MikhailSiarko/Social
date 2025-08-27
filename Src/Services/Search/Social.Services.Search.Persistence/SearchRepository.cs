using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Social.Services.Search.Application;
using Social.Services.Search.Application.Models;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.Search.Persistence;

public sealed class SearchRepository : ISearchRepository
{
    private readonly string _indexName;
    private readonly ElasticsearchClient _client;

    public SearchRepository(IConfiguration configuration)
    {
        var settings = new ElasticsearchClientSettings(new Uri(configuration.GetConnectionString("ElasticSearch:Url")!));
        _indexName = configuration["ElasticSearch:UserIndex"]!;
        settings.DefaultIndex(_indexName);
        _client = new ElasticsearchClient(settings);
    }

    public async Task<Result<Unit>> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await CreateIndexIfNotExistsAsync(cancellationToken);
            var response = await _client.IndexAsync(user, x => x.Index(_indexName), cancellationToken);
            if (response.IsSuccess())
                return Unit.Value;
            return new Error("Error while adding user to search index: {UserID}", user.Id);
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while adding user to search index: {UserID}", user.Id);
        }
    }

    public async Task<Result<Unit>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await CreateIndexIfNotExistsAsync(cancellationToken);
            var response =
                await _client.UpdateAsync<User, User>(_indexName, user.Id, x => x.Doc(user), cancellationToken);
            if (response.IsSuccess())
                return Unit.Value;
            return new Error("Error while updating search index: {UserID}", user.Id);
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while updating search index: {UserID}", user.Id);
        }
    }

    public async Task<Result<Guid[]>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            await CreateIndexIfNotExistsAsync(cancellationToken);
            var response = await _client.SearchAsync<User>(x =>
            {
                x.Indices(_indexName).Query(q =>
                        q.MultiMatch(mm => mm.Fields(
                                f => f.FirstName,
                                f => f.LastName,
                                f => f.UserName,
                                f => f.Email)
                            .Query(query)
                            .Fuzziness(ff => ff.Value("AUTO")))
                    )
                    .Size(10);
            }, cancellationToken);
            if (response.IsSuccess())
                return response.Documents.Select(x => x.Id).ToArray();
            return new Error("Error while searching users: {Message}", response.DebugInformation);
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while searching users");
        }
    }

    private async Task CreateIndexIfNotExistsAsync(CancellationToken token = default)
    {
        var existsResult = await _client.Indices.ExistsAsync(_indexName, token);
        if (existsResult.Exists)
            return;

        await _client.Indices.CreateAsync<User>(_indexName,
            x =>
            {
                x.Mappings(a => a.Properties(p =>
                    p.Text(v => v.Email)
                        .Text(v => v.FirstName)
                        .Text(v => v.LastName)
                        .Text(v => v.UserName)));
            }, cancellationToken: token);
    }
}