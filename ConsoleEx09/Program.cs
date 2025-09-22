/*

     1~9사이의 정수 n을 읽어들여 해당하는 구구단의 n단 출력

*/

using System;

namespace Ex09 {
    class Program {
        static void Main(string[] args) {

            Console.Write("구구단의 단수를 입력하세요: ");
            int n = int.Parse(Console.ReadLine());
            
            for(int i=1; i<10; i++) {
                Console.WriteLine("{0} * {1} = {2}",n,i,(n*i));
            }
        }
    }
}
