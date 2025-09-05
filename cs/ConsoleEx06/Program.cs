/*

    1부터 100까지 더하기 (for, do while, while문 사용)

*/

using System;

namespace Ex06 {
    class Program {
        static void Main(string[] args) {
            int sum = 0;



            // for문
            // for (int i=1; i<=100; i++) {
            //     sum += i;
            // }



            // do while문
            // int i = 1;

            // do {
            //     sum += i;
            //     i++;
            // } while(i<=100);



            // while문
            int i = 1;

            while(i<=100) {
                sum += i;
                i++;
            }



            Console.WriteLine("1부터 100까지 더하기: {0}",sum);
        }
    }
}
