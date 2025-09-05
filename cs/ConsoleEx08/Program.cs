/*

    1부터 128까지의 숫자를 2진수, 8진수, 16진수로 출력

*/

using System;

namespace Ex08 {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("10진수 \t 2진수 \t\t 8진수 \t 16진수 \t");

            for(int i=1; i<=128; i++) {
                Console.WriteLine("{0} \t {1} \t {2} \t {3} \t",
                i,
                Convert.ToString(i,2).PadLeft(8, '0'),
                Convert.ToString(i,8),
                Convert.ToString(i,16));
            }
        }
    }
}