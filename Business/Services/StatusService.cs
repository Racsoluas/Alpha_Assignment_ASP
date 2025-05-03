using Business.Models;
using Data.Repositories;
using Domain.Models;

namespace Business.Services;

public interface IStatusService
{
    Task<StatusResult<StatusModel>> GetStatusByIdAsync(int id);
    Task<StatusResult<StatusModel>> GetStatusByNameAsync(string statusName);
    Task<StatusResult<IEnumerable<StatusModel>>> GetStatusesAsync();
}

public class StatusService(IStatusRepository statusRepository) : IStatusService
{
    private readonly IStatusRepository _statusRepository = statusRepository;

    public async Task<StatusResult<IEnumerable<StatusModel>>> GetStatusesAsync()
    {
        var result = await _statusRepository.GetAllAsync();
        return result.Succeeded
            ? new StatusResult<IEnumerable<StatusModel>> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusResult<IEnumerable<StatusModel>> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<StatusResult<StatusModel>> GetStatusByNameAsync(string statusName)
    {
        var result = await _statusRepository.GetAsync(x => x.StatusName == statusName);
        return result.Succeeded
            ? new StatusResult<StatusModel> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusResult<StatusModel> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<StatusResult<StatusModel>> GetStatusByIdAsync(int id)
    {
        var result = await _statusRepository.GetAsync(x => x.Id == id);
        return result.Succeeded
            ? new StatusResult<StatusModel> { Succeeded = true, StatusCode = 200, Result = result.Result }
            : new StatusResult<StatusModel> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }
}