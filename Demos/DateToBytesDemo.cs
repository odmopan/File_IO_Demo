namespace FileIoDemo.Demos;

/// <summary>
/// Демонстрація: збереження дати в масиві байтів.
/// Формат масиву з 4 елементів:
/// [0] — день, [1] — місяць, [2] і [3] — рік (молодший і старший байти).
/// </summary>
public static class DateToBytesDemo
{
    public static void Run()
    {
        Console.WriteLine("Збереження бінарних даних");
        Console.WriteLine();

        DateTime date = new DateTime(2025, 9, 1);
        Console.WriteLine($"Початкова дата: {date:dd.MM.yyyy}");

        byte[] bytes = DateToBytes(date);
        PrintBytes(bytes);

        DateTime restored = BytesToDate(bytes);
        Console.WriteLine($"Відновлена дата: {restored:dd.MM.yyyy}");
        Console.WriteLine($"Дати однакові: {date == restored}");
    }

    private static byte[] DateToBytes(DateTime date)
    {
        byte[] bytes = new byte[4];

        bytes[0] = (byte)date.Day;
        bytes[1] = (byte)date.Month;

        // Рік не вміщується в один байт (максимум 255), тому займає два байти.
        // BitConverter.GetBytes повертає масив з 2 байтів для значення типу ushort.
        byte[] yearBytes = BitConverter.GetBytes((ushort)date.Year);
        bytes[2] = yearBytes[0];
        bytes[3] = yearBytes[1];
        

        return bytes;
    }

    private static DateTime BytesToDate(byte[] bytes)
    {
        int day = bytes[0];
        int month = bytes[1];
        // Зчитуємо 2 байти, починаючи з індексу 2
        ushort year = BitConverter.ToUInt16(bytes, 2);

        return new DateTime(year, month, day);
    }

    private static void PrintBytes(byte[] bytes)
    {
        string[] names = { "день", "місяць", "рік (молодший байт)", "рік (старший байт)" };

        Console.WriteLine();
        Console.WriteLine("Масив байтів:");
        for (int i = 0; i < bytes.Length; i++)
        {
            Console.WriteLine($"  [{i}] = {bytes[i],3} (0x{bytes[i]:X2}, {Convert.ToString(bytes[i], 2).PadLeft(8, '0')})  — {names[i]}");
        }
        Console.WriteLine();
    }
}
