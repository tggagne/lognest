using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace LogNest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var worker = new MainClass();

            worker.work(args);

            Console.Out.Flush();
            Environment.Exit(0);
        }

        public void work(string[] args)
        {
            var methodStack = new Stack<string>();

            if (args.Length == 0)
                usage();

            Console.WriteLine(String.Format("Opening file [{0}]", args[0]));

            using (var readStream = System.IO.File.OpenText(args[0]))
            {
                string aLine;
                int tabs = 0, lineNumber = 1;
                var methodMap = new Dictionary<string, int>();

                var exitPattern = new Regex("\\|METHOD_EXIT|CONSTRUCTOR_EXIT\\|");
                var enterPattern = new Regex("\\|METHOD_ENTRY|CONSTRUCTOR_ENTRY\\|");
                var echoPattern = new Regex("(^[0-9:.]{12} \\([0-9]{9}\\)\\|)");
                var debugPattern = new Regex("\\|USER_DEBUG\\|");

                while ((aLine = readStream.ReadLine()) != null)
                {
                    if (exitPattern.IsMatch(aLine))
                    {
                        tabs--;
                        methodStack.Pop();
                    }

                    if (enterPattern.IsMatch(aLine))
                    {
                        tab(lineNumber, tabs++);
                        Console.Out.WriteLine(aLine);

                        var pieces = aLine.Split('|');
                        var method = pieces[pieces.Length - 1];
                        if (methodMap.ContainsKey(method) == false)
                            methodMap[method] = 0;

                        methodMap[method]++;
                        methodStack.Push(method);
                    }

                    if (debugPattern.IsMatch(aLine) || echoPattern.IsMatch(aLine) == false)
                        Console.Out.WriteLine(aLine);

                    lineNumber++;
                }

                var methodCount = methodStack.Count;
                for (int i = 0; i < methodCount; i++)
                    Console.Out.WriteLine("{0} {1}", methodCount - (i + 1), methodStack.Pop());

                var aList = new List<KeyValuePair<string, int>>();
                foreach (var each in methodMap)
                    aList.Add(each);

                aList.Sort(comparator);
                foreach (var each in aList)
                    Console.Out.WriteLine("{0, 5} {1}", each.Value, each.Key);
            }
        }

        int comparator(KeyValuePair<string, int> left, KeyValuePair<string, int> right)
        {
            if (left.Value < right.Value)
                return 1;

            if (left.Value == right.Value)
                return 0;

            return -1;
        }

        void tab(int lineNumber, int anInt)
        {
            int i;

            Console.Out.Write("{0} {1}  ", lineNumber, anInt);

            for (i = 0; i < anInt; i++)
                Console.Out.Write("    ");
        }

        void usage()
        {
            Console.Error.WriteLine("usage: lognest pathname");
            Environment.Exit(1);
        }
    }
}