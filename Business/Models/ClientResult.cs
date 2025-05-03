using Domain.Models;

namespace Business.Models;
public class ClientResult : ServiceResult
{
    public IEnumerable<ClientModel>? Result { get; set; }
    public ClientModel? SingleResult { get; set; }
}