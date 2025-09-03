/* 

        BitwiseOperators 

*/

using System;

class Hello
{
    static void Main()
    {
        int x = 14, y = 11, result;

        Console.WriteLine(x + " | " + y + " = " + (x | y));
        Console.WriteLine(x + " & " + y + " = " + (x & y));
        Console.WriteLine(x + " ^ " + y + " = " + (x ^ y));
        Console.WriteLine("~" + x + " = " + (~x));
        Console.WriteLine(x + " << " + y + " = " + (x << 2));
        Console.WriteLine(x + " >> " + y + " = " + (x >> 1));
    }
}

/*

14(10) = 1110(2)
11(10) = 1011(2)

1110(2) | 1011(2) = 1111(2) = 8 + 4 + 2 + 1 = 15(10)
1110(2) & 1011(2) = 1010(2) = 8 + 2 = 10
1110(2) ^ 1011(2) = 0101(2) = 4 + 1 = 5

14 << 2 = 0000 1110(2) = 0011 1000(2) = 32 + 16 + 8 = 56(10)
11 >> 1 = 0000 1011(2) = 0000 0101(2) = 4 + 1 = 5(10)

*/