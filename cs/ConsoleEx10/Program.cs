/*

    5명의 키를 읽은 후 평균과 최소, 최대값을 구하여 출력

*/

using System;

namespace Ex10 {
    class Program {
        static void Main(string[] args) {
            
            double max = double.MinValue; //최대값을 찾는 알고리즘의 초기값으로 사용
            double min = double.MaxValue; //최소값을 찾는 알고리즘의 초기값으로 사용

            double sum = 0;

            for(int i=0; i<5; i++) {
                Console.Write("키를 입력하세요(단위: cm): ");

                double h = double.Parse(Console.ReadLine());

                if (h > max) 
                    max = h;
                if (h < min)
                    min = h;
                sum += h;
            }

            Console.WriteLine("평균: {0,6}, 최대: {1}, 최소: {2}", (sum/5), max, min);
        }
    }
}

/*

서식 지정자 {} 
- 출력할 값을 정렬하고 공간을 확보
- {n,m} 에서 n은 인덱스, m은 공간(폭)을 의미 
- {0,6} 은 0번째 값을 6칸으로 출력
- {1} 은 1번째 값을 기본 폭으로 출력
- {2} 는 2번째 값을 기본 폭으로 출력
- 폭이 양수면 오른쪽 정렬, 음수면 왼쪽 정렬

*/