using Network;
using Xunit;
using Xunit.Abstractions;

namespace DevTools.Tests;

public class IpRangeTest
{
    protected readonly ITestOutputHelper Output;

    public IpRangeTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void Print()
    {
        var range = IpRange.Parse("10.0.0.0/19");
        
        var (one, two, three, four, five, six, seven, eight) = range.Split8();
        var (nodes, _) = one.Split2();

        var systemPods = two;
        var defaultPods = three;
        var windowsPods = four;
        Output.WriteLine($"Vnet: {range} ({range.Size})");
        Output.WriteLine($"nodes: {nodes} ({nodes.Size})");
        Output.WriteLine($"systemPods: {systemPods} ({systemPods.Size})");
        Output.WriteLine($"defaultPods: {three} ({three.Size})");
        Output.WriteLine($"windowsPods: {four} ({four.Size})");

        var serviceRange = IpRange.Parse("172.16.0.0/20");
        
        Output.WriteLine($"SeriviceCidr: {serviceRange} ({serviceRange.Size}) firstIp: {serviceRange.FirstIp()} lastIp: {serviceRange.LastIp()}");
    }

    [Fact]
    public void Print2()
    {
        var range = IpRange.Parse("0.0.0.0/0");
        var (sub1, sub2) = range.Split2();
        PrettyPrint(range);
        PrettyPrint(sub1);
        PrettyPrint(sub2);
    }

    private void PrettyPrint(IpRange range)
    {
        Output.WriteLine($"nodes: {range} ({range.Size})");
    }
}