/*

    Array Class

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex11 {
    class Program {
        static void Main(string[] args) {


            // Initialize
            int[] a = {5, 25, 75, 35, 15};
            PrintArray(a);


            // Copy Array
            int[] b;
            b = (int[])a.Clone();
            PrintArray(b);

            int[] c = new int[10];
            Array.Copy(a, 0, c, 1, 3);
            PrintArray(c);

            a.CopyTo(c, 3);
            PrintArray(c);


            // Ascending
            Array.Sort(a);
            PrintArray(a);


            // Descending
            Array.Reverse(a);
            PrintArray(a);


            // Initialize
            Array.Clear(a, 0, a.Length);
            PrintArray(a);
        }

        private static void PrintArray(int[] a) {
            foreach(var i in a)
                Console.Write("{0,5}", i);
            Console.WriteLine();
        }
    }
}