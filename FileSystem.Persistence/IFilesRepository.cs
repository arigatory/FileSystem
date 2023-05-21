using FileSystem.Persistence.Models;

namespace FileSystem.Persistence
{
    public interface IFolderRepository
    {
        Guid Add(Folder folder);
        Task<Folder> GetWithoutChildrenAsync(Guid id);
        Task<Folder> GetWithChildrenAsync(Guid id);
        Task<int> SaveAsync();
        void Update(Folder folder);

        Task<IReadOnlyList<Folder>> GetAllAsync();
        Task<Folder> GetTreeStargingFromIdSeveralRequests(Guid id);
        Task<Folder> GetTreeStartingFromIdAsync(Guid id);
    }
}