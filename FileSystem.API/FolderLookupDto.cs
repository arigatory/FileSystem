namespace FileSystem.API
{
    public class FolderLookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TotalChildren { get; set; }
    }
}
