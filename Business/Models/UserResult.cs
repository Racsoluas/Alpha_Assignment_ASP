using Domain.Models;

namespace Business.Models;

public class UserResult : ServiceResult
{
    public IEnumerable<UserModel>? Result { get; set; }
    public UserModel? SingleResult { get; set; }
}