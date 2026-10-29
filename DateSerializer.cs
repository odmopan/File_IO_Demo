namespace FileIoDemo;

/// <summary>
/// Перетворює дату на 4 байти і навпаки, у масив або в потік.
/// Формат: [0] — день, [1] — місяць, [2] і [3] — рік (молодший і старший байти).
/// </summary>
public static class DateSerializer
{
    public const int Size = 4;

    public static byte[] Serialize(DateTime date)
    {
        byte[] bytes = new byte[Size];

        bytes[0] = (byte)date.Day;
        bytes[1] = (byte)date.Month;

        byte[] yearBytes = BitConverter.GetBytes((ushort)date.Year);
        bytes[2] = yearBytes[0];
        bytes[3] = yearBytes[1];

        return bytes;
    }

    public static DateTime Deserialize(byte[] bytes)
    {
        if (bytes.Length != Size)
        {
            throw new ArgumentException($"Для дати потрібно рівно {Size} байти, отримано {bytes.Length}.", nameof(bytes));
        }

        int day = bytes[0];
        int month = bytes[1];
        ushort year = BitConverter.ToUInt16(bytes, 2);

        return new DateTime(year, month, day);
    }

    public static void Serialize(BinaryWriter writer, DateTime date)
    {
        writer.Flush();

        writer.Write((byte)date.Day);
        writer.Write((byte)date.Month);
        writer.Write((ushort)date.Year);
    }

    public static DateTime Deserialize(Stream stream)
    {
        BinaryReader reader = new BinaryReader(stream);
        
        int day = reader.ReadByte();
        int month =  reader.ReadByte();
        int year= reader.ReadUInt16();
        
        return new DateTime(year, month, day);
    }
}
