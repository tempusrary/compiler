namespace Tempusrary.Compiler.Library.Utils;

public class PathInfo(string path)
{
    public string Path { get; set; } = path;

    // Write methods to create a directory or file and return a new PathInfo object
    public PathInfo CreateDirectory(string name) => new(Directory.CreateDirectory(System.IO.Path.Combine(Path, name)).FullName);
    public PathInfo CreateFile(string file)
    {
        File.WriteAllText(System.IO.Path.Combine(Path, file), "");
        return new PathInfo(System.IO.Path.Combine(Path, file));
    }

    public bool IsEmpty => Directory.EnumerateFileSystemEntries(Path).Any();

    public DirectoryInfo GetDirectoryInfo() => new(Path);
    public FileInfo GetFileInfo() => new(Path);
}