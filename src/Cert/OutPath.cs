namespace Cert;

public readonly struct OutPath
{
    public string Raw { get; }
    public bool HasExtension => Path.HasExtension(Raw);
    public string Extension => Path.GetExtension(Raw)?.Trim('.').ToLowerInvariant();
    public bool IsDirectory => Directory.Exists(Raw);

    public string ParentDirectory => Path.GetDirectoryName(Raw);
    public string Name => Path.GetFileName(Raw);
    public string NameWithoutExtension => Path.GetFileNameWithoutExtension(Raw);

    public OutPath(string raw)
    {
        Raw = raw;
    }

    public override string ToString()
    {
        return Raw;
    }

    public static implicit operator OutPath(string raw) => new(raw);
    public static implicit operator string(OutPath path) => path.Raw;

    public OutPath WithExtension(string extension)
    {
        return new OutPath(Path.Combine(ParentDirectory, $"{Path.GetFileNameWithoutExtension(Raw)}.{extension.Trim('.')}"));
    }

    public OutPath Combine(string part)
    {
        return Path.Combine(Raw, part);
    }
}