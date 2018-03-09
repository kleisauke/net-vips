using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetVips.Samples
{
    class Program
    {
        public static List<IGrouping<string, ISample>> Samples = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(ISample)) && x.GetConstructor(Type.EmptyTypes) != null)
            .Select(x => Activator.CreateInstance(x) as ISample)
            .GroupBy(s => s.Category)
            .ToList();

        static void Main(string[] args)
        {
            if (!Base.VipsInit())
            {
                Console.WriteLine("Unable to init libvips");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("libvips " + Base.Version(0) + "." + Base.Version(1) + "." + Base.Version(2));

            Console.WriteLine("Type an item number to execute the specified sample. Type exit to quit.");
            Console.WriteLine();
            Console.WriteLine("Menu:");

            var index = 1;
            foreach (var group in Samples)
            {
                Console.WriteLine($" - {group.Key}");
                foreach (var item in group)
                {
                    Console.WriteLine($"    {index}: {item.Name}");
                    index++;
                }

                Console.WriteLine();
            }

            string input;
            do
            {
                input = Console.ReadLine();
                if (int.TryParse(input, out var userChoice) && TryGetSample(userChoice, out var sample))
                {
                    Console.WriteLine($"Executing sample: {sample.Name}");
                    var result = sample.Execute(args);
                    Console.WriteLine("Sample successfully executed!");
                    if (result != null)
                    {
                        Console.WriteLine($"Result: {result}");
                    }
                }
                else
                {
                    Console.WriteLine("Sample doesn't exists, try again");
                }
            } while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryGetSample(int id, out ISample sample)
        {
            var index = 1;
            foreach (var group in Samples)
            {
                foreach (var item in group)
                {
                    if (index == id)
                    {
                        sample = item;
                        return true;
                    }

                    index++;
                }
            }

            sample = null;
            return false;
        }
    }
}