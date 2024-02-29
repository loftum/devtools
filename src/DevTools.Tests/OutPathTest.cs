using Cert;
using Xunit;

namespace DevTools.Tests;

public class OutPathTest
{
    [Fact]
    public void OutPath()
    {
        OutPath path = "/some/path/to/file.txt";
        
        Assert.Equal("/some/path/to/file.txt", path.Raw);
        Assert.Equal("/some/path/to", path.ParentDirectory);
        Assert.Equal("file.txt", path.Name);
        Assert.Equal("file", path.NameWithoutExtension);
    }

    [Fact]
    public void WithExtension()
    {
        OutPath pem = "/some/path/to/file.pem";

        var key = pem.WithExtension("key");
        Assert.Equal("/some/path/to/file.key", key);
    }
}