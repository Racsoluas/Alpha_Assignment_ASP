using Data.Contexts;
using Data.Entities;
using Data.Models;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Data.Repositories;

public interface IProjectRepository : IBaseRepository<ProjectEntity, Project>
{
    Task<RepositoryResult<ProjectEntity>> GetEntityAsync(string id);

}

public class ProjectRepository(DataContext context) : BaseRepository<ProjectEntity, Project>(context), IProjectRepository
{
    public override async Task<RepositoryResult<IEnumerable<Project>>> GetAllAsync(
    bool orderByDescending = false,
    Expression<Func<ProjectEntity, object>>? sortBy = null,
    Expression<Func<ProjectEntity, bool>>? where = null,
    params Expression<Func<ProjectEntity, object>>[] includes)
    {
        IQueryable<ProjectEntity> query = _table;

        if (where != null)
            query = query.Where(where);

        query = query.Include(p => p.Client)
                     .Include(p => p.Status)
                     .Include(p => p.User);

        if (includes != null && includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (sortBy != null)
            query = orderByDescending
                ? query.OrderByDescending(sortBy)
                : query.OrderBy(sortBy);

        var entities = await query.ToListAsync();

        var result = entities.Select(p => new Project
        {
            Id = p.Id,
            Image = p.Image,
            ProjectName = p.ProjectName,
            Created = p.Created,
            Budget = p.Budget,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            StatusId = p.StatusId,
            Client = p.Client != null ? new ClientModel
            {
                Id = p.Client.Id,
                ClientName = p.Client.ClientName
            } : null,
            Status = p.Status != null ? new StatusModel
            {
                Id = p.Status.Id,
                StatusName = p.Status.StatusName
            } : null,
            User = p.User != null ? new UserModel
            {
                Id = p.User.Id,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Image = p.User.Image
            } : null
        });

        return new RepositoryResult<IEnumerable<Project>>
        {
            Succeeded = true,
            StatusCode = 200,
            Result = result
        };
    }

    public async Task<RepositoryResult<ProjectEntity>> GetEntityAsync(string id)
    {
        var entity = await _table.FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
            return new RepositoryResult<ProjectEntity> { Succeeded = false, StatusCode = 404, Error = "Project not found." };

        return new RepositoryResult<ProjectEntity> { Succeeded = true, StatusCode = 200, Result = entity };
    }
}