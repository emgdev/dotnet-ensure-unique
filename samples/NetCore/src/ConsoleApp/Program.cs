using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Loading...");

            await Task.Delay(TimeSpan.FromSeconds(10));

            Console.WriteLine("Bye!");
        }
    }
}
