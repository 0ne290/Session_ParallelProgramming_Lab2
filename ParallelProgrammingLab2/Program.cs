using System.Diagnostics;
using System.Text;
using Bogus;
using Bogus.DataSets;
using Newtonsoft.Json;

namespace ParallelProgrammingLab2;

internal static class Program
{
    private static int Main()
    {
        try
        {
            /*Console.WriteLine(
                $"Важное примечание про пулы потоков: Для них помимо, собственно, кол-ва параллельных потоков нужно еще" +
                $" вводить и кол-во задач. В случае с текущей реализацией прохода по массиву, задача - проход по всем" +
                $" записям телефонного справочника. Эта задача диктуется содержимым метода" +
                $" Phonebook.GetSubscribersByLastName(). Если взглянуть на этот метод, то становится ясно, что выход из" +
                $" него (т. е. завершение задачи) происходит только после полного прохода по всем записям. Таким образом," +
                $" отработка 4 таких задач пулом потоков с 2 потоками будет происходить так: все 4 задачи встанут в" +
                $" очередь; пул направит 2 из них на обработку двумя своими потоками; задачи будут выполняться параллельно" +
                $" до того момента, как все записи не будут пройдены (таким образом, конечное время прохода по записям" +
                $" будет в два раза меньше в сравнении с однопоточным проходом). На этот момент имеем следующую ситуацию:" +
                $" 2 задачи уже выполнены и еще 2 задачи стоят в очереди, но проход по записям уже полностью завершен" +
                $" двумя этими выполненными задачами; пул ставит на обработку оставшиеся 2 задачи, но они сразу же" +
                $" завершаются, т. к. проход по записям уже завершен.{Environment.NewLine}");*/
            
            Console.WriteLine($"Файл \"Input.txt\" содержит сериализованные записи телефонного справочника. Файл \"Parameters.json\" содержит все остальные входные параметры.{Environment.NewLine}");

            Console.Write("Сгенерировать файл \"Input.txt\" (любая непустая последовательность, если да)? ");
            if (!string.IsNullOrWhiteSpace(Console.ReadLine()))
            {
                Console.Write(
                    "Сколько записей генерировать (если введете не натуральное число или вовсе не число, то сгенерируется 5 000 000 записей)? ");
                if (!(int.TryParse(Console.ReadLine(), out var numberOfRandomTelephoneSubscribers) &&
                      numberOfRandomTelephoneSubscribers > 0))
                    numberOfRandomTelephoneSubscribers = 5_000_000;

                Console.WriteLine("Файл генерируется. Пожалуйста, подождите...");
                WriteRandomTelephoneSubscribersToFile("../../../Input.txt", numberOfRandomTelephoneSubscribers);
                Console.WriteLine("Файл сгенерирован.");
            }

            Console.Write(
                "Перед тем, как начнется выполнение основной части программы, у Вас есть возможность внести изменения " +
                "в файлы \"Input.txt\" и \"Parameters.json\" - воспользуйтесь ею сейчас, если хотите. Для продолжения нажмите любую клавишу... ");
            Console.ReadKey();
            Console.WriteLine($"{Environment.NewLine}Выполняется обход массива записей телефонного справочника. Пожалуйста, подождите...");
            
            var input = JsonConvert.DeserializeObject<InputData>(File.ReadAllText("../../../Parameters.json"));
            
            if (input == null)
                throw new Exception(
                    "Невозможно прочитать данные из файла конфигурации. Вероятно, данные не соответствуют формату.");
            if (input.Handler is < 1 or > 3 || input.Synchronizer is < 1 or > 3 || input.PauseTime < 1)
                throw new Exception("Значение одного из входных параметров некорректно.");
            if (input.ThreadsNumber < 1)
                input.ThreadsNumber = Environment.ProcessorCount;

            ISynchronizationPrimitive synchronizationPrimitive = input.Synchronizer switch
            {
                1 => new CriticalSection(),
                2 => new BinarySemaphore(),
                3 => new PetriNet.BinarySemaphore(),
                _ => throw new Exception(
                    "Примитива синхронизации под таким номером не существует. Этой ошибки не должно быть.")
            };

            var reader = new InputFileReader();
            var multithreadPhonebookDecorator =
                new MultithreadPhonebookDecorator(new Phonebook(synchronizationPrimitive,
                    reader.Read("../../../Input.txt", Encoding.UTF8), input.PauseTime));

            var stopwatch = new Stopwatch();
            var writer = new StreamWriter("../../../Output.txt", false, Encoding.UTF8);
            IEnumerable<TelephoneSubscriber> subscribers = null!;

            switch (input.Handler)
            {
                case 1:
                    stopwatch.Start();
                    subscribers = multithreadPhonebookDecorator.GetSubscribersByLastNameUsingThreadArray(input.Surnames, 1);
                    writer.WriteLine($"Massive. 1 thread. Time: {stopwatch.ElapsedMilliseconds}; Result:");

                    foreach (var subscriber in subscribers)
                        writer.WriteLine(
                            $"FirstName: {subscriber.FirstName}; LastName: {subscriber.LastName}; PhonuNumber: {subscriber.PhoneNumber}; Address: {subscriber.Address}");

                    stopwatch.Restart();
                    subscribers =
                        multithreadPhonebookDecorator.GetSubscribersByLastNameUsingThreadArray(input.Surnames, input.ThreadsNumber);
                    writer.WriteLine(
                        $"{Environment.NewLine}Massive. {input.ThreadsNumber} threads. Time: {stopwatch.ElapsedMilliseconds}; Result:");
                    break;
                case 2:
                    ThreadPool.SetMinThreads(input.ThreadsNumber, input.ThreadsNumber);

                    stopwatch.Start();
                    subscribers =
                        multithreadPhonebookDecorator.GetSubscribersByLastNameUsingSystemThreadPool(input.Surnames, 1);
                    writer.WriteLine(
                        $"System ThreadPool. 1 thread. Time: {stopwatch.ElapsedMilliseconds}; Result:");

                    foreach (var subscriber in subscribers)
                        writer.WriteLine(
                            $"FirstName: {subscriber.FirstName}; LastName: {subscriber.LastName}; PhonuNumber: {subscriber.PhoneNumber}; Address: {subscriber.Address}");

                    stopwatch.Restart();
                    subscribers =
                        multithreadPhonebookDecorator.GetSubscribersByLastNameUsingSystemThreadPool(input.Surnames,
                            input.ThreadsNumber);
                    writer.WriteLine(
                        $"{Environment.NewLine}System ThreadPool. {input.ThreadsNumber} threads. Time: {stopwatch.ElapsedMilliseconds}; Result:");
                    break;
                case 3:
                    var threadPool = new PetriNet.ThreadPool.ThreadPool(input.ThreadsNumber);

                    stopwatch.Start();
                    subscribers =
                        multithreadPhonebookDecorator.GetSubscribersByLastNameUsingCustomThreadPool(input.Surnames, 1,
                            threadPool);
                    writer.WriteLine(
                        $"Custom ThreadPool. 1 thread. Time: {stopwatch.ElapsedMilliseconds}; Result:");

                    foreach (var subscriber in subscribers)
                        writer.WriteLine(
                            $"FirstName: {subscriber.FirstName}; LastName: {subscriber.LastName}; PhonuNumber: {subscriber.PhoneNumber}; Address: {subscriber.Address}");

                    stopwatch.Restart();
                    subscribers =
                        multithreadPhonebookDecorator.GetSubscribersByLastNameUsingCustomThreadPool(input.Surnames,
                            input.ThreadsNumber,
                            threadPool);
                    writer.WriteLine(
                        $"{Environment.NewLine}Custom ThreadPool. {input.ThreadsNumber} threads. Time: {stopwatch.ElapsedMilliseconds}; Result:");

                    threadPool.Dispose();
                    break;
            }

            foreach (var subscriber in subscribers)
                writer.WriteLine(
                    $"FirstName: {subscriber.FirstName}; LastName: {subscriber.LastName}; PhonuNumber: {subscriber.PhoneNumber}; Address: {subscriber.Address}");

            writer.Dispose();
            if (synchronizationPrimitive is IDisposable disposableSynchronizationPrimitive)
                disposableSynchronizationPrimitive.Dispose();
            
            Console.Write("Обход успешно завершен. Нажмите любую клавишу для завершения программы...");
            Console.ReadKey();

            return 0;
        }
        catch (Exception e)
        {
            Console.Write($"Ошибка. {e.Message} Нажмите любую клавишу для завершения программы...");
            Console.ReadKey();

            return 1;
        }
    }
    
    private static void WriteRandomTelephoneSubscribersToFile(string path, int numberOfRandomTelephoneSubscribers)
    {
        var faker = new Faker("ru");

        var writer = new StreamWriter(path, false, Encoding.UTF8);

        for (var i = 0; i < numberOfRandomTelephoneSubscribers - 1; i++)
        {
            writer.Write($"{faker.Name.FirstName(Name.Gender.Male)}; ");
            writer.Write($"{faker.Name.LastName(Name.Gender.Male)}; ");
            writer.Write($"{faker.Phone.PhoneNumber("+7 (###) ###-##-##")}; ");
            writer.WriteLine(
                $"{faker.Address.ZipCode()}, г. {faker.Address.City()}, {faker.Address.StreetAddress()}, кв. {faker.Random.Int(1, 100)}");
        }
        
        writer.Write($"{faker.Name.FirstName(Name.Gender.Male)}; ");
        writer.Write($"{faker.Name.LastName(Name.Gender.Male)}; ");
        writer.Write($"{faker.Phone.PhoneNumber("+7 (###) ###-##-##")}; ");
        writer.Write(
            $"{faker.Address.ZipCode()}, г. {faker.Address.City()}, {faker.Address.StreetAddress()}, кв. {faker.Random.Int(1, 100)}");

        writer.Dispose();
    }
}