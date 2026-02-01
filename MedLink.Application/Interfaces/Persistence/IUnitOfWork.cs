using MedLink.Domain.Common;

namespace MedLink.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> Complete();
        IUserProfileRepository UserRepository { get; }
    }
}
