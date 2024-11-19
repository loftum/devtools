using System.Text;

namespace Jwk.Encodings;

public static class Base64
{
    public static string Encode(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        return Convert.ToBase64String(bytes);
    }

    public static string Encode(string value)
    {
        return Encode(Encoding.UTF8.GetBytes(value));
    }

    public static byte[] Decode(string base64)
    {
        if (base64 == null)
        {
            throw new ArgumentNullException(nameof(base64));
        }
        return Convert.FromBase64String(base64);
    }

    public static string UrlEncode(string value)
    {
        return Encode(Encoding.UTF8.GetBytes(value));
    }

    public static string? UrlEncode(byte[]? bytes)
    {
        if (bytes == null)
        {
            return null;
        }
        return new StringBuilder(Convert.ToBase64String(bytes))
            .Replace("=", string.Empty)
            .Replace('+', '-')
            .Replace('/', '_')
            .ToString();
    }

    public static byte[] UrlDecode(string base64UrlFriendly)
    {
        if (base64UrlFriendly == null)
        {
            throw new ArgumentNullException(nameof(base64UrlFriendly));
        }
        var padLength = (4 - base64UrlFriendly.Length % 4) % 4;
        var value = new StringBuilder(base64UrlFriendly)
            .Replace('-', '+')
            .Replace('_', '/')
            .Append('=', padLength);
        return Decode(value.ToString());
    }

    public static string ToUtf8(string base64)
    {
        var bytes = Decode(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    public static string FromUtf8(string utf8)
    {
        var bytes = Encoding.UTF8.GetBytes(utf8);
        return Encode(bytes);
    }
    
    /// <summary>
    /// Encodes UTF-8 bytes to base64 url-encoded bytes.
    /// Base64 url-encoding uses - instead of + and _ instead of /. And there is no padding (=)
    /// https://fm4dd.com/programming/base64/base64_algorithm.shtm
    ///
    /// In short: Every 3 bytes (24 bits) are transformed to 4 6-bit "bytes"
    /// Example (3 bytes. Underscore indicates where the bits are split up):
    /// 155 -> 100110_11
    /// 162 -> 1010_0010
    /// 233 -> 11_101001
    ///
    /// transform to:
    ///
    /// 100110 -> 38
    /// 111010 -> 58
    /// 001011 -> 11
    /// 101001 -> 41
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>base64 url-encoded bytes in UTF-8 (=ASCII)</returns>
    public static byte[] UrlEncodeToUtf8Bytes(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var byteCount = stream.Length;
        var remainder = byteCount % 3;
        var length = byteCount / 3 * 4 + remainder switch { 1 => 2, 2 => 3, _ => 0};
        var array = new byte[length];
        
        var buffer = new byte[3];
        int read;
        var index = 0;
        
        // Read 3 bytes at a time, until there are none left.
        // When only 1 or 2 bytes are read, these are the last "remainder" bytes
        while ((read = stream.Read(buffer)) > 0)
        {
            // This is Andreas' favourite kind of code. Shifting and masking. We all know it.
            switch (read)
            {
                case 1:
                {
                    // 1 remainder byte
                    // Example:
                    // 155 -> 100110_11
                    //
                    // transforms to
                    //
                    // 100110 -> 38
                    // 110000  -> 48
                    var first = (byte) (buffer[0] >> 2 & 0b00111111);
                    var second = (byte)(buffer[0] << 4 & 0b00110000);
                    
                    array[index++] = ToUtf8Byte(first);
                    array[index++] = ToUtf8Byte(second);
                    break;
                }
                case 2:
                {
                    // 2 remainder bytes
                    // Example:
                    // 155 -> 100110_11
                    // 162 -> 1010_0010
                    //
                    // transform to
                    //
                    // 100110 -> 38
                    // 111010 -> 58
                    // 001000 -> 8
                    
                    var first = (byte) (buffer[0] >> 2 & 0b00111111);
                    var second = (byte)(buffer[0] << 4 & 0b00110000 | buffer[1] >> 4 & 0b00001111);
                    var third = (byte) (buffer[1] << 2 & 0b00111100);
                    
                    array[index++] = ToUtf8Byte(first);
                    array[index++] = ToUtf8Byte(second);
                    array[index++] = ToUtf8Byte(third);
                    
                    break;
                }
                case 3:
                {
                    // "Normal" case. 3 bytes are perfectly transformed to 4 6-bit "bytes". Lovely.
                    //
                    // Example:
                    // 155 -> 100110_11
                    // 162 -> 1010_0010
                    // 233 -> 11_101001
                    // 
                    // transforms to
                    //
                    // 100110 -> 38
                    // 111010 -> 58
                    // 001011 -> 11
                    // 101001 -> 41
                    
                    var first = (byte) (buffer[0] >> 2 & 0b00111111);
                    var second = (byte)(buffer[0] << 4 & 0b00110000 | buffer[1] >> 4 & 0b00001111);
                    var third = (byte) (buffer[1] << 2 & 0b00111100 | buffer[2] >> 6 & 0b00000011);
                    var fourth = (byte) (buffer[2] & 0b00111111);
                    
                    array[index++] = ToUtf8Byte(first);
                    array[index++] = ToUtf8Byte(second);
                    array[index++] = ToUtf8Byte(third);
                    array[index++] = ToUtf8Byte(fourth);
                    
                    break;
                }
            }
        }

        return array;
    }

    public static byte[] UrlEncodeToUtf8Bytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return UrlEncodeToUtf8Bytes(stream);
    }

    // A-Z, a-z, 0-9 and - and _
    private static readonly byte[] Base64UrlToUtf8 =
    {
        65, 66, 67, 68, 69, 70, 71, 72,
        73, 74, 75, 76, 77, 78, 79, 80,
        81, 82, 83, 84, 85, 86, 87, 88,
        89, 90, 97, 98, 99, 100, 101, 102,
        103, 104, 105, 106, 107, 108, 109, 110,
        111, 112, 113, 114, 115, 116, 117, 118,
        119, 120, 121, 122, 48, 49, 50, 51,
        52, 53, 54, 55, 56, 57, 45, 95
    };
    
    private static byte ToUtf8Byte(byte base64)
    {
        if (base64 > 63)
        {
            throw new IndexOutOfRangeException($"{base64} is invalid");
        }
        return Base64UrlToUtf8[base64];
    }
}