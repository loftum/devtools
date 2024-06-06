using System.Text;

namespace Network;

public readonly struct IpRange
{
    private readonly uint _address;
    private readonly uint _min;
    private readonly uint _max;
    private readonly byte _cidrBits;
    
    /// <summary>
    /// number of IPs in this range
    /// </summary>
    public ulong Size => (ulong) (_max - _min) + 1;

    private IpRange(uint address, byte cidrBits)
    {
        _address = address;
        _cidrBits = cidrBits;
        
        uint mask = 0;
        
        for (var ii=0; ii< cidrBits; ii++)
        {
            mask |= (uint)1 << (31 - ii);
        }

        _min = address & mask;
        _max = _min | (mask ^ uint.MaxValue);
    }

    /// <summary>
    /// Splits this IpRange in 2 (sub) IpRanges
    /// </summary>
    /// <returns>2 (sub) IpRanges</returns>
    public (IpRange, IpRange) Split2()
    {
        var part = Split(2);
        return (part[0], part[1]);
    }

    /// <summary>
    /// Splits this IpRange into 4 (sub) IpRanges
    /// </summary>
    /// <returns>4 (sub) IpRanges</returns>
    public (IpRange, IpRange, IpRange, IpRange) Split4()
    {
        var part = Split(4);
        return (part[0], part[1], part[2], part[3]);
    }
    
    /// <summary>
    /// Splits this IpRange into 8 (sub) IpRanges.
    /// </summary>
    /// <returns>8 (sub) IpRanges</returns>
    public (IpRange, IpRange, IpRange, IpRange, IpRange, IpRange, IpRange, IpRange) Split8()
    {
        var part = Split(8);
        return (part[0], part[1], part[2], part[3], part[4], part[5], part[6], part[7]);
    }

    /// <summary>
    /// Splits this IPRange into a given number of (sub) IpRanges. Number must be a power of 2.
    /// </summary>
    /// <param name="number">Power of 2</param>
    /// <returns>an array of (sub) IpRanges</returns>
    public IpRange[] Split(byte number)
    {
        if (number % 2 != 0)
        {
            throw new ArgumentException($"Cannot split into {number}. Must be positive and dividable by 2");
        }
        
        var cidr = _cidrBits + (byte)Math.Log2(number);
        if (cidr > 32)
        {
            throw new InvalidOperationException($"Cidr is already at {_cidrBits}. Can't divide into {number} more. ({cidr})");
        }
        
        var chunk = (uint)(Size / number);
        var subRanges = new IpRange[number];
        for (var ii = 0; ii < number; ii++)
        {
            subRanges[ii] = new IpRange(_min + (uint) ii * chunk, (byte) cidr);
        }

        return subRanges;
    }
    
    public string FirstIp()
    {
        var octets = GetOctets(_min);
        return string.Join('.', octets);
    }
    
    public string LastIp()
    {
        var octets = GetOctets(_max);
        return string.Join('.', octets);
    }

    public string Format()
    {
        var octets = GetOctets(_address);
        var ip = string.Join('.', octets);
        return $"{ip}/{_cidrBits}";
    }

    public static implicit operator string(IpRange range) => range.Format();

    private static byte[] GetOctets(uint address)
    {
        var octets = new byte[4];
        octets[0] = (byte)((address & 0xff000000) >> 24);
        octets[1] = (byte)((address & 0x00ff0000) >> 16);
        octets[2] = (byte)((address & 0x0000ff00) >> 8);
        octets[3] = (byte)((address & 0x000000ff) >> 0);
        return octets;
    }

    public override string ToString() => Format();

    public static IpRange Parse(string cidr)
    {
        var octets = new uint[4];
        var builder = new StringBuilder();
        var cidrBits = byte.MaxValue;

        var octetIndex = 0;
        var cidrIndex = 0;
        
        foreach(var c in cidr)
        {
            cidrIndex++;
            switch (c)
            {
                case '.':
                    if (octetIndex >= 3)
                    {
                        throw new ArgumentException($"Invalid CIDR '{cidr}'. Four octets is enough at {cidrIndex}", nameof(cidr));
                    }

                    octets[octetIndex] = byte.Parse(builder.ToString());
                    builder.Clear();
                    octetIndex++;
                    break;
                case '/':
                    
                    if (octetIndex < 3)
                    {
                        throw new ArgumentException($"Invalid CIDR '{cidr}'. Expected more numbers at {cidrIndex}", nameof(cidr));
                    }

                    octets[octetIndex] = byte.Parse(builder.ToString());
                    builder.Clear();
                    octetIndex++;
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }

        if (octetIndex != 4 && builder.Length > 0)
        {
            octets[octetIndex] = byte.Parse(builder.ToString());
            builder.Clear();
            octetIndex++;
        }
        
        if (octetIndex != 4)
        {
            throw new ArgumentException($"Invalid CIDR '{cidr}'. Expected more numbers at {cidrIndex}", nameof(cidr));
        }
       

        if (builder.Length > 0)
        {
            cidrBits = byte.Parse(builder.ToString());
        }

        var address = octets[0] << 24 | octets[1] << 16 | octets[2] << 8 | octets[3];

        return new IpRange(address, cidrBits);
    }

    public static bool TryParse(string value, out IpRange range)
    {
        try
        {
            range = Parse(value);
            return true;
        }
        catch
        {
            range = default;
            return false;
        }
    }

    public bool ContainsIp(string ip)
    {
        return TryParse(ip, out var range) &&
               range._min >= _min &&
               range._max <= _max;
    }
}