//Buổi 2
//Bài 3: Tính biểu thức nhanh 1
using System;

class Bai3
{
    static void Main()
    {
        Console.Write("nhap so thuc x: ");
        double x = double.Parse(Console.ReadLine());

        //Đặt x làm thừa số chung để tính biểu thức nhanh
        double t1 = -4 * x + 3;   
        double t2 = t1 * x + 2;   
        double f = t2 * x + 1;    
        Console.WriteLine($"f({x}) = {f}");
    }
}