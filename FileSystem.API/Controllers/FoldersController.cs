using FileSystem.Persistence;
using FileSystem.Persistence.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoldersController : ControllerBase
{
    private readonly IFolderRepository _folderRepository;

    public FoldersController(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Folder>>> GetAll()
    {
        var folders = await _folderRepository.GetAllAsync();
        return Ok(folders);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Folder>> Get(Guid id)
    {
        var folder = await _folderRepository.GetWithoutChildrenAsync(id);
        return Ok(folder);
    }

    [HttpGet("auto/{id}")]
    public async Task<ActionResult<Folder>> GetAuto(Guid id)
    {
        var folder = await _folderRepository.GetWithChildrenAsync(id);
        return Ok(folder);
    }

    [HttpGet("lookup/{id}")]
    public async Task<ActionResult<FolderLookupDto>> GetLookup(Guid id)
    {
        var folder = await _folderRepository.GetWithoutChildrenAsync(id);
        var folderDto = new FolderLookupDto
        {
            Name = folder.Name,
            Id = folder.Id,
            TotalChildren = folder.Children.Count,
        };
        return Ok(folderDto);
    }


    [HttpGet("tree-one-sql/{id}")]
    public async Task<ActionResult<Folder>> GetTreeOneSql(Guid id)
    {
        var folder = await _folderRepository.GetTreeStartingFromIdAsync(id);
        return Ok(folder);
    }

    [HttpGet("tree-several-sqls/{id}")]
    public async Task<ActionResult<Folder>> GetTreeSeveralSqls(Guid id)
    {
        var folder = await _folderRepository.GetTreeStargingFromIdSeveralRequests(id);
        return Ok(folder);
    }

    [HttpPost]
    public async Task<IActionResult> Add(FolderDto folderDto)
    {
        if (folderDto.ParentId.HasValue)
        {
            var parent = await _folderRepository.GetWithoutChildrenAsync(folderDto.ParentId.Value);
            if (parent == null)
            {
                return BadRequest($"Не найден родитель: {folderDto.ParentId.Value}");
            }
        }

        var folder = new Folder(folderDto.Name, folderDto.ParentId);

        _folderRepository.Add(folder);

        await _folderRepository.SaveAsync();

        return NoContent();
    }

    [HttpPost("edit")]
    public async Task<IActionResult> EditFromId(Guid id)
    {
        var foldersToEdit = await _folderRepository.GetTreeStartingFromIdAsync(id);
        foreach (var item in foldersToEdit.Children)
        {
            item.Name += "-------------";
        }

        //_folderRepository.Update(foldersToEdit);

        await _folderRepository.SaveAsync();

        return NoContent();
    }

    [HttpPost("copy")]
    public async Task<IActionResult> Copy(Guid idToCopy, Guid destinationId)
    {
        var folderToCopy = await _folderRepository.GetTreeStartingFromIdAsync(idToCopy);
        var destinationFolder = await _folderRepository.GetWithChildrenAsync(destinationId);

        destinationFolder.Children.Add(folderToCopy.Clone());

        await _folderRepository.SaveAsync();

        return NoContent();
    }

}