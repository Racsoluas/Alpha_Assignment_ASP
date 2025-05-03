using Data.Contexts;
using Data.Entities;
using Data.Models;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public interface IClientRepository : IBaseRepository<ClientEntity, ClientModel>
{
    Task<RepositoryResult<ClientEntity>> GetEntityAsync(string id);
}

public class ClientRepository(DataContext context) : BaseRepository<ClientEntity, ClientModel>(context), IClientRepository
{
    public async Task<RepositoryResult<ClientEntity>> GetEntityAsync(string id)
    {
        var entity = await _table.FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
            return new RepositoryResult<ClientEntity> { Succeeded = false, StatusCode = 404, Error = "Client not found." };

        return new RepositoryResult<ClientEntity> { Succeeded = true, StatusCode = 200, Result = entity };
    }
}