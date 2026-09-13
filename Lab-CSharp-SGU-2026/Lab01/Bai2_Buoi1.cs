//Buổi 1
//bài 1: Tính diện tích và chu vi hình tròn  
using System;
class Bai2{
    static void Main(){
        Console.WriteLine("Nhập bán kính hình tròn: ");
        //Nhập bán kính hình tròn
        double r = Convert.ToDouble(Console.ReadLine());
        //Tính diện tích hình tròn
        double S = Math.PI * r * r;
        //Tính chu vi hình tròn
        double C = 2 * Math.PI * r;
        Console.WriteLine("Diện tích hình tròn là: " + S);
        Console.WriteLine("Chu vi hình tròn là: " + C);
    }
}