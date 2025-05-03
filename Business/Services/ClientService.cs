using Business.Models;
using Data.Entities;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;

namespace Business.Services;

public interface IClientService
{
    Task<ClientResult> CreateClientAsync(AddClientForm formData);
    Task<ClientResult> DeleteClientAsync(string id);
    Task<ClientResult> GetClientAsync(string id);
    Task<ClientResult> GetClientsAsync();
    Task<ClientResult> UpdateClientAsync(EditClientForm formData);
}

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<ClientResult> CreateClientAsync(AddClientForm formData)
    {
        if (formData == null)
            return new ClientResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var clientEntity = formData.MapTo<ClientEntity>();

        var result = await _clientRepository.AddAsync(clientEntity);

        return result.Succeeded
            ? new ClientResult { Succeeded = true, StatusCode = 201 }
            : new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ClientResult> GetClientsAsync()
    {
        var result = await _clientRepository.GetAllAsync();
        return result.MapTo<ClientResult>();
    }

    public async Task<ClientResult> GetClientAsync(string id)
    {
        var result = await _clientRepository.GetAsync(x => x.Id == id);

        if (!result.Succeeded || result.Result == null)
            return new ClientResult { Succeeded = false, StatusCode = 404, Error = $"Client '{id}' was not found." };

        var clientModel = result.Result.MapTo<ClientModel>();

        return new ClientResult
        {
            Succeeded = true,
            StatusCode = 200,
            SingleResult = clientModel
        };
    }

    public async Task<ClientResult> UpdateClientAsync(EditClientForm formData)
    {
        if (formData == null)
            return new ClientResult { Succeeded = false, StatusCode = 400, Error = "Invalid form data." };

        var existingClientResult = await _clientRepository.GetEntityAsync(formData.Id);

        if (!existingClientResult.Succeeded)
            return new ClientResult { Succeeded = false, StatusCode = 404, Error = "Client not found." };

        var clientEntity = existingClientResult.Result!;

        clientEntity.ClientName = formData.ClientName;
        clientEntity.Email = formData.Email;
        clientEntity.Location = formData.Location;
        clientEntity.Phone = formData.Phone;

        if (!string.IsNullOrEmpty(formData.Image))
        {
            clientEntity.Image = formData.Image;
        }

        var result = await _clientRepository.UpdateAsync(clientEntity);

        return result.Succeeded
            ? new ClientResult { Succeeded = true, StatusCode = 200 }
            : new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ClientResult> DeleteClientAsync(string id)
    {
        var existingEntity = await _clientRepository.GetEntityAsync(id);
        if (!existingEntity.Succeeded || existingEntity.Result == null)
        {
            return new ClientResult
            {
                Succeeded = false,
                StatusCode = 404,
                Error = $"Client with ID '{id}' not found."
            };
        }

        var result = await _clientRepository.DeleteAsync(existingEntity.Result);
        return result.Succeeded
            ? new ClientResult { Succeeded = true, StatusCode = 200 }
            : new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }
};