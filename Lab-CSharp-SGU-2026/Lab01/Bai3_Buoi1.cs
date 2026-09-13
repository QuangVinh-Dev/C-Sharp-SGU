using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Nhập số giờ:");
        int Gio = int.Parse(Console.ReadLine());
        Console.WriteLine("Nhập số phút:");
        int Phut = int.Parse(Console.ReadLine());
        Console.WriteLine("Nhập số giây:");
        int Giay = int.Parse(Console.ReadLine());
        int TongGiay = Gio * 3600 + Phut * 60 + Giay;
        Console.WriteLine("Tổng số giây là: " + TongGiay);
    }

}