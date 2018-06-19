using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;

namespace Samples
{
    class Program
    {
        #region digit sperators

        private const int fiftyMillion = 50_000_000;
        private const int bitmask = 0b_0001_0011;

        #endregion

        #region async main

        static async Task Main(string[] args)
        {
            WriteLine("It's gonna be legen...");
            await Task.Delay(500);
            WriteLine("...wait for it...");
            await Task.Delay(1000);
            WriteLine("...dary!");

            ReadLine();
        }

        #endregion

        #region throw expressions

        private readonly string[] args;

        public Program(string[] args)
        {
            this.args = args ?? throw new ArgumentNullException(nameof(args));
        }

        #endregion

        #region default

        static async Task Default()
        {
            CancellationToken token = default;
            await Task.Run(() => WriteLine("No need for the type."), token);
        }

        #endregion

        #region out var

        static void OutVar()
        {
            if (int.TryParse(ReadLine(), out var number))
            {
                WriteLine("You entered: " + number);
            }

            WriteLine("You can also use number here:" + number);
        }

        #endregion

        #region pattern matching

        static bool IsNaN(object number)
        {
            switch (number)
            {
                case float f:
                    return float.IsNaN(f);

                case double d:
                    return double.IsNaN(d);

                case string s when double.TryParse(s, out var d):
                    return double.IsNaN(d);

                default:
                    return false;
            }

            // Can also be used with if:

            if (number is string s2 && double.TryParse(s2, out var d2) && double.IsInfinity(d2))
            {
                return true;
            }
        }

        #endregion

        #region expression bodied members

        class PositiveValue
        {
            private int value;

            public int Value
            {
                get => this.value;
                set => this.value = value >= 0 ? value : throw new ArgumentException("Value must not be negative");
            }

            public PositiveValue(int value) => this.Value = value;
        }

        #endregion

        #region local functions 

        class TreeItem
        {
            public IList<TreeItem> Children { get; private set; } = new List<TreeItem>();
        }

        IEnumerable<TreeItem> FlattenTree(TreeItem root)
        {
            var items = new List<TreeItem>();

            // Here we define a local function in the FlattenTree method.
            // Just like with closures, we can access any variables that are currently in scope.
            void AddRecursive(TreeItem item) 
            {
                items.Add(item);
                foreach (var child in item.Children)
                {
                    AddRecursive(child);
                }
            }

            AddRecursive(root);

            return items;
        }

        #endregion

        #region tuples

        // This method returns a tuple (two named int values).
        static (int width, int height) GetSize(Rectangle r)
        {
            return (r.Width, r.Height);
        }

        // This method expects a tuple of two ints as parameter.
        static void PrintSize((int width, int height) size)
        {
            WriteLine($"{size.width}; {size.height}");
        }

        static void Tuples()
        {
            var r = new Rectangle(5, 5, 10, 10);

            // The names actually don't really matter. They are just aliases for Item1 and Item2.
            // If the types match, two tuples are compatible and can be assigned to variables.

            // Assign to tuple without names:
            (int, int) size = GetSize(r);
            PrintSize(size);

            // Assign to tuple with other names:
            (int w, int h) size2 = GetSize(r);
            PrintSize(size2);
            
            // We can also assign to variables directly.

            // Assign to previously declared variables:
            int w; int h;
            (w, h) = GetSize(r);
            PrintSize((w, h));

            // Assign to variables in definition:
            var (w3, h3) = size;
            PrintSize((w3, h3));
        }

        #endregion        
        
        #region ref return and ref local

        class ChartSeries
        {
            private readonly (int x, int y)[] dataPoints;

            public ChartSeries(params (int x, int y)[] dataPoints)
            {
                this.dataPoints = dataPoints;
            }

            // This method returns a reference to the Y coordinate of a data point.
            // That means you can assign new values.
            public ref int FindDataPoint(int x)
            {
                for (int i = 0; i < dataPoints.Length; i++)
                {
                    if (dataPoints[i].x == x)
                    {
                        return ref dataPoints[i].y;
                    }
                }

                throw new ArgumentException("DataPoint not found.");
            }

            public void Print()
            {
                foreach (var dataPoint in dataPoints)
                {
                    Write(dataPoint.y);
                }
            }
        }

        static void Ref()
        {
            var series = new ChartSeries((1, 3), (2, 4), (3, 4), (4, 1));

            // Get a reference to the data point at x = 3...
            ref int yAt3 = ref series.FindDataPoint(3);

            // .. and assign a new value.
            yAt3 = 5;

            series.Print();
        }

        #endregion

        #region spans

        static void Spans()
        {
            var array = new int[100];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }

            // A span is like a window into an array.
            var from10To20 = new Span<int>(array, 10, 10);
            var from5To25 = new Span<int>(array, 5, 20);

            Square(from10To20);
            Print(from5To25);
        }

        static void Square(Span<int> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                // Just like with refs, we can modify the values directly...
                span[i] *= span[i];
            }
        }

        // ...except when we use a read-only span.
        static void Print(ReadOnlySpan<int> span)
        {
            foreach (var item in span)
            {
                WriteLine(item);
            }
        }

        #endregion

        #region private protected

        // This is a class intended for public use deriving from an abstract base class.
        public sealed class DerivedClassForPublicUse : AbstractBaseClass
        {
            void SomePublicMethod()
            {
                // It calls a protected method of the base class, passing an internal enum.
                ProtectedMethodIntendedForDerivedClass(InternalEnum.Option1);
            }
        }

        // This enum is internal as it is only used as parameter for the abstract base class method.
        // We don't want to make it visible to other assemblies.
        internal enum InternalEnum
        {
            Option1,
            Option2
        }

        // This is the abstract base class. It has to be public because the derived class is public.
        public abstract class AbstractBaseClass
        {
            // We only want to expose this method for our special derived class.
            // Using protected instead of private protected doesn't work, because then it
            // would be visible to derived classed from other assemblies as well, which
            // can't see the internal enum.
            private protected void ProtectedMethodIntendedForDerivedClass(InternalEnum option)
            {
                WriteLine(option);
            }
        }

        #endregion
    }
}
