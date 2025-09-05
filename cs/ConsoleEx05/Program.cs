/*

    StringBuffer 

*/

using System;

namespace Ex05 {
    class Program {
        static void Main(string[] args) {

            string buffer = "The numbers are: ";

            for (int i = 0; i < 3; i++) {
                buffer += i.ToString();
                Console.WriteLine(buffer);
            }

            
        }
    }
}

/* 

String 객체가 자주 변경되는 경우에는 StringBuilder를 사용
StringBuilder는 String을 추가, 삭제, 수정하는 Method를 제공

*/