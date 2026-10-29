using System.Text;

namespace FileIoDemo.Demos;

/// <summary>
/// Демонстрація: робота з текстовим файлом на файловій системі.
/// Створення файла, відкриття на читання й на редагування,
/// запис рядка і масиву рядків, дописування в кінець, вставлення рядка.
/// </summary>
public static class TextFileDemo
{
    public static void Run()
    {
        Console.WriteLine("Робота з текстовим файлом");
        Console.WriteLine();

        string directory = Path.Combine(Path.GetTempPath(), "FileIoDemo");
        string path = Path.Combine(directory, "demo.txt");
        Console.WriteLine($"Шлях до файла: {path}");
        Console.WriteLine();

        try
        {
            Directory.CreateDirectory(directory);

            CreateFile(path);
            PrintFileInfo(path);

            CreateFileWithFinally(path);
            PrintFileInfo(path);

            WriteText(path, "Перший рядок");
            PrintContent(path, "Після запису рядка");

            WriteTextWithStream(path, "Перший рядок, записаний через потік");
            PrintContent(path, "Після запису рядка через FileStream і StreamWriter");

            WriteLines(path, new string[] { "Рядок 1", "Рядок 3", "Рядок 4" });
            PrintContent(path, "Після запису масиву рядків (попередній вміст замінено)");

            WriteLinesWithStream(path, new string[] { "Рядок 1", "Рядок 3", "Рядок 4" });
            PrintContent(path, "Після запису масиву рядків через FileStream і StreamWriter");

            AppendText(path, "Рядок 5, дописаний в кінець");
            PrintContent(path, "Після дописування в кінець");

            AppendTextWithStream(path, "Рядок 6, дописаний через потік");
            PrintContent(path, "Після дописування в кінець через FileStream і StreamWriter");

            InsertSecondLine(path, "Рядок 2, вставлений другим");
            PrintContentWithStream(path, "Після вставлення другого рядка (читання потоку рядок за рядком)");

            OpenForReading(path);
            OpenForEditing(path);
            PrintContent(path, "Після редагування файла на місці");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
    }

    /// <summary>
    /// Створює порожній файл. Якщо файл уже існує, його вміст буде втрачено.
    /// </summary>
    private static void CreateFile(string path)
    {
        // File.Create повертає FileStream, який треба закрити, інакше файл залишиться заблокованим
        using (FileStream stream = File.Create(path))
        {
            Console.WriteLine($"Файл створено, довжина = {stream.Length} байтів");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Те саме, що CreateFile, але потік закривається явно в блоці finally.
    /// Оператор using розгортається компілятором саме в таку конструкцію.
    /// </summary>
    private static void CreateFileWithFinally(string path)
    {
        FileStream? stream = null;
        try
        {
            stream = File.Create(path);
            Console.WriteLine($"Файл створено, довжина = {stream.Length} байтів");
        }
        finally
        {
            // Перевірка на null потрібна: якщо File.Create кине виняток, stream залишиться null
            if (stream != null)
            {
                stream.Close();
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Записує один рядок. Попередній вміст файла замінюється.
    /// </summary>
    private static void WriteText(string path, string text)
    {
        File.WriteAllText(path, text);
    }

    /// <summary>
    /// Те саме, що WriteText, але файл відкривається як потік, а текст пишеться через StreamWriter.
    /// </summary>
    private static void WriteTextWithStream(string path, string text)
    {
        // FileMode.Create — створити файл, а якщо він є, то очистити його
        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(text);

            // StreamWriter накопичує дані в буфері; Flush скидає їх у потік і зберігає у файл
            writer.Flush();
            Console.WriteLine($"Після Flush у файлі {stream.Length} байтів");
        }
        // using закриває спочатку writer, потім stream
        Console.WriteLine();
    }

    /// <summary>
    /// Записує масив рядків, кожен у окремому рядку файла. Попередній вміст файла замінюється.
    /// </summary>
    private static void WriteLines(string path, string[] lines)
    {
        File.WriteAllLines(path, lines);
    }

    /// <summary>
    /// Те саме, що WriteLines, але файл відкривається як потік, а рядки пишуться через StreamWriter.
    /// </summary>
    private static void WriteLinesWithStream(string path, string[] lines)
    {
        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            foreach (string line in lines)
            {
                // WriteLine додає після рядка символ переведення рядка
                writer.WriteLine(line);
            }

            writer.Flush();
            Console.WriteLine($"Після Flush у файлі {stream.Length} байтів");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Дописує рядок у кінець файла, не чіпаючи наявного вмісту.
    /// </summary>
    private static void AppendText(string path, string text)
    {
        // append: true — потік відкривається в режимі дописування
        using (StreamWriter writer = new StreamWriter(path, append: true))
        {
            writer.WriteLine(text);
        }
    }

    /// <summary>
    /// Те саме, що AppendText, але файл відкривається як потік, а рядок пишеться через StreamWriter.
    /// </summary>
    private static void AppendTextWithStream(string path, string text)
    {
        // FileMode.Append — відкрити файл (створити, якщо його немає) і встановити позицію в кінець.
        // Цей режим дозволений лише разом з FileAccess.Write.
        using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            Console.WriteLine($"Позиція одразу після відкриття = {stream.Position}");

            writer.WriteLine(text);

            writer.Flush();
            Console.WriteLine($"Після Flush позиція = {stream.Position}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Вставляє рядок другим у файл: зчитує всі рядки, вставляє новий за індексом 1
    /// і записує файл заново. Вставити всередину файла безпосередньо не можна,
    /// бо рядки мають різну довжину, і все після місця вставки треба зсувати.
    /// </summary>
    private static void InsertSecondLine(string path, string text)
    {
        List<string> lines = new List<string>(File.ReadAllLines(path));

        int index = Math.Min(1, lines.Count);
        lines.Insert(index, text);

        File.WriteAllLines(path, lines);
    }

    /// <summary>
    /// Відкриває файл лише на читання: FileMode.Open (файл має існувати), FileAccess.Read.
    /// </summary>
    private static void OpenForReading(string path)
    {
        Console.WriteLine("Відкриття на читання (FileMode.Open, FileAccess.Read):");

        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (StreamReader reader = new StreamReader(stream))
        {
            Console.WriteLine($"  CanRead = {stream.CanRead}, CanWrite = {stream.CanWrite}");
            Console.WriteLine($"  Перший рядок: {reader.ReadLine()}");

            try
            {
                stream.WriteByte(0);
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine($"  Спроба запису: {ex.GetType().Name} — потік відкрито лише на читання");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Відкриває файл на читання і запис: FileMode.Open, FileAccess.ReadWrite.
    /// Змінює початок файла на місці, не перезаписуючи весь файл.
    /// </summary>
    private static void OpenForEditing(string path)
    {
        Console.WriteLine("Відкриття на редагування (FileMode.Open, FileAccess.ReadWrite):");

        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
        {
            Console.WriteLine($"  CanRead = {stream.CanRead}, CanWrite = {stream.CanWrite}");

            // Позиція після відкриття — початок файла, тому запис замінить перші байти.
            // Нові байти мають бути тієї самої довжини, інакше зіпсуємо наступні дані.
            byte[] bytes = Encoding.UTF8.GetBytes("РЯДОК");
            stream.Write(bytes, 0, bytes.Length);

            Console.WriteLine($"  Замінено перші {bytes.Length} байтів, позиція = {stream.Position}");
        }
        Console.WriteLine();
    }

    private static void PrintFileInfo(string path)
    {
        FileInfo info = new FileInfo(path);
        Console.WriteLine($"Файл існує: {info.Exists}, розмір = {info.Length} байтів");
        Console.WriteLine();
    }

    /// <summary>
    /// Те саме, що PrintContent, але файл відкривається як потік і читається по одному рядку,
    /// кожен рядок одразу виводиться на екран. У пам'яті є лише поточний рядок, а не весь файл.
    /// </summary>
    private static void PrintContentWithStream(string path, string title)
    {
        Console.WriteLine($"{title}:");

        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (StreamReader reader = new StreamReader(stream))
        {
            int number = 1;
            string? line = reader.ReadLine();
            while (line != null)
            {
                Console.WriteLine($"  {number}: {line}");
                number++;

                // ReadLine повертає null, коли файл закінчився
                line = reader.ReadLine();
            }
        }
        Console.WriteLine();
    }

    private static void PrintContent(string path, string title)
    {
        Console.WriteLine($"{title}:");

        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            Console.WriteLine($"  {i + 1}: {lines[i]}");
        }
        Console.WriteLine();
    }
}
