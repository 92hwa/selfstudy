/*

    1부터 100까지 홀수의 합, 역수의 합

*/

using System;

namespace Ex07 {
    class Program {
        static void Main(string[] args) {

            //홀수의 합 
            int sum_odd = 0;

            for(int i=1; i<=100; i++) {
                if(i%2==1) {
                    sum_odd += i;
                }
            }

            Console.WriteLine("홀수의 합: {0}", sum_odd);



            //역수의 합
            double sum_frac = 0;

            for(int i=1; i<=100; i++) {
                sum_frac += 1.0/i;
            }

            Console.WriteLine("역수의 합: {0}", sum_frac);
        }
    }
}