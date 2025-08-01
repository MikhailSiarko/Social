using Social.Services.Search.Application.Models;
using Social.Shared;

namespace Social.Services.Search.Application;

public interface ISearchRepository
{
    Task<Result<Unit>> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<Unit>> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<Guid[]>> SearchAsync(string query, CancellationToken cancellationToken = default);
}