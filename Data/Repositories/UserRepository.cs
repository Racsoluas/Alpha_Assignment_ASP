using Data.Contexts;
using Data.Entities;
using Domain.Models;

namespace Data.Repositories;

public interface IUserRepository : IBaseRepository<UserEntity, UserModel>
{
}

public class UserRepository(DataContext context) : BaseRepository<UserEntity, UserModel>(context), IUserRepository
{
}