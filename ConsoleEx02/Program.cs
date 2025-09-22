/*

    Conditional Operator
*/

using System;

namespace Ex02
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Input: ");

            int input = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("{0}은 " + ((input % 2 == 0) ? "짝수입니다." : "홀수입니다."), input);
        }
    }
}

