namespace FileIoDemo.Demos;

/// <summary>
/// Демонстрація: запис і читання кількох дат через MemoryStream.
/// Кожна дата займає 4 байти (день, місяць, рік у двох байтах),
/// дати йдуть у потоці одна за одною без роздільників.
/// </summary>
public static class DatesInMemoryStreamDemo
{
    private const int DateSize = 4;

    public static void Run()
    {
        Console.WriteLine("Збереження кількох дат у потоці MemoryStream");
        Console.WriteLine();

        DateTime[] dates =
        {
            new DateTime(2025, 9, 1),
            new DateTime(2025, 12, 31),
            new DateTime(2026, 1, 15),
            new DateTime(1999, 5, 20)
        };

        using (MemoryStream stream = new MemoryStream())
        {
            Console.WriteLine($"Новий потік: довжина = {stream.Length}, позиція = {stream.Position}");
            Console.WriteLine();

            Console.WriteLine("Запис дат:");
            foreach (DateTime date in dates)
            {
                WriteDate(stream, date);
                Console.WriteLine($"  записано {date:dd.MM.yyyy}: довжина = {stream.Length}, позиція = {stream.Position}");
            }
            Console.WriteLine();

            // Після запису позиція стоїть у кінці потоку, читати нема що.
            // Щоб читати з початку, позицію треба повернути на 0.
            stream.Position = 0;
            Console.WriteLine($"Позицію повернуто на початок: позиція = {stream.Position}");
            Console.WriteLine();

            Console.WriteLine("Читання дат:");
            int count = Count(stream);
            Console.WriteLine($"  кількість дат у потоці = {count}");
            for (int i = 0; i < count; i++)
            {
                DateTime date = ReadDate(stream);
                Console.WriteLine($"  прочитано {date:dd.MM.yyyy}: позиція = {stream.Position}");
            }
            Console.WriteLine();

            Console.WriteLine("Доступ за індексом (у довільному порядку):");
            int[] indexes = { 2, 0, 3, 1 };
            foreach (int index in indexes)
            {
                DateTime date = GetByIndex(stream, index);
                Console.WriteLine($"  [{index}] = {date:dd.MM.yyyy}: позиція = {stream.Position}");
            }
            Console.WriteLine();

            PrintBytes(stream.ToArray());
        }
    }

    private static void WriteDate(Stream stream, DateTime date)
    {
        byte[] bytes = new byte[DateSize];

        bytes[0] = (byte)date.Day;
        bytes[1] = (byte)date.Month;

        byte[] yearBytes = BitConverter.GetBytes((ushort)date.Year);
        bytes[2] = yearBytes[0];
        bytes[3] = yearBytes[1];

        stream.Write(bytes, 0, bytes.Length);
    }

    private static DateTime ReadDate(Stream stream)
    {
        byte[] bytes = new byte[DateSize];

        // Read може повернути менше байтів, ніж запитано, тому перевіряємо результат
        int read = stream.Read(bytes, 0, bytes.Length);
        if (read != DateSize)
        {
            throw new EndOfStreamException("У потоці недостатньо байтів для читання дати.");
        }

        int day = bytes[0];
        int month = bytes[1];
        ushort year = BitConverter.ToUInt16(bytes, 2);

        return new DateTime(year, month, day);
    }

    private static int Count(Stream stream)
    {
        return (int)(stream.Length / DateSize);
    }

    private static DateTime GetByIndex(Stream stream, int index)
    {
        if (index < 0 || index >= Count(stream))
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Дати з таким індексом немає в потоці.");
        }

        // Усі дати мають однаковий розмір, тому зсув дорівнює індексу, помноженому на розмір дати
        long offset = (long)index * DateSize;
        stream.Position = offset;

        return ReadDate(stream);
    }

    private static void PrintBytes(byte[] bytes)
    {
        Console.WriteLine($"Увесь вміст потоку ({bytes.Length} байтів):");
        for (int i = 0; i < bytes.Length; i += DateSize)
        {
            Console.WriteLine($"  {bytes[i],3} {bytes[i + 1],3} {bytes[i + 2],3} {bytes[i + 3],3}");
        }
    }
}
