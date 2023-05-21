using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;

namespace FileSystem.Persistence.Models;

public class Folder
{
    private ICollection<Folder> _children;

    public Folder(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    public Folder()
    {
        
    }
    public Folder(string name)
    {
        Name = name;
    }

    public Folder(string name, Guid? parentId)
    {
        Name = name;
        ParentId = parentId;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Folder> Children
    {
        get => LazyLoader.Load(this, ref _children);
        set => _children = value;
    }

    public Guid? ParentId { get; set; }

    private ILazyLoader LazyLoader { get; set; }





    public Folder Clone()
    {
        var clone = new Folder(Name);

        foreach (var child in Children)
        {
            clone.Children.Add(child.Clone());
        }

        return clone;
    }
}
