using FileSystem.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FileSystem.Persistence
{
    public class FolderRepository : IFolderRepository
    {
        private readonly DatabaseContext _context;

        public FolderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Guid Add(Folder folder)
        {
            _context.Add(folder);
            return folder.Id;
        }

        public async Task<Folder> GetWithoutChildrenAsync(Guid id)
        {
            return await _context.Folders
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Folder> GetWithChildrenAsync(Guid id)
        {
            return await _context.Folders
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Folder>> GetAllAsync()
        {
            var result = await _context.Folders.ToListAsync();
            return result;
        }

        // returned folder isn't tracked by EF
        public static Expression<Func<Folder, Folder>> GetFolderProjection(int maxDepth, int currentDepth = 0)
        {
            currentDepth++;
            Expression<Func<Folder, Folder>> result = folder => new Folder
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.ParentId,
                Children = currentDepth == maxDepth
                ? new List<Folder>()
                : folder.Children.AsQueryable()
                   .Select(GetFolderProjection(maxDepth, currentDepth))
                   .OrderBy(y => y.Name).ToList()
            };

            return result;
        }
        

        public async Task<Folder> GetTreeStartingFromIdAsync(Guid id)
        {
            var query = _context
                .Folders
                .Where(x => x.Id == id)
                .Select(GetFolderProjection(14, 0))
                .OrderBy (y => y.Name);

            return (await query.ToListAsync()).FirstOrDefault();
        }


        public async Task<Folder> GetTreeStargingFromIdSeveralRequests(Guid id)
        {
            var result = await LoadFolderWithChildrenAsync(id);

            return result;
        }

        private async Task<Folder> LoadFolderWithChildrenAsync(Guid id)
        {
            var folder = await _context.Folders.FindAsync(id);

            if (folder != null)
            {
                await _context.Entry(folder).Collection(f => f.Children).LoadAsync();

                foreach (var child in folder.Children)
                {
                    await LoadFolderWithChildrenAsync(child.Id);
                }
            }

            return folder;
        }

        public void Update(Folder folder)
        {
            _context.Folders.Update(folder);
        }
    }
}
