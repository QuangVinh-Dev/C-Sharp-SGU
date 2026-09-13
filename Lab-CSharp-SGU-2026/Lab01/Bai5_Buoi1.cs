using System;
using System.Globalization;

class Bai5_Buoi1
{
    public static void Main(string[] args)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        try
        {
            Console.Write("Nhap a :");
            double a =double.Parse(Console.ReadLine());

            Console.Write("Nhap b :");
            double b =double.Parse(Console.ReadLine());

            Console.Write("Nhap c :");
            double c =double.Parse(Console.ReadLine());

            if (a <= 0 || b <= 0 || c <= 0 || (a + b <= c) || (a + c <= b) || (b + c <= a))
                {
                    Console.WriteLine("Loi: Ba so vua nhap khong the tao thanh mot tam giac hop le.");
                    return;
                }
            double p = (a + b + c) / 2;

            double S = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

            Console.WriteLine($"Dien tich tam giac S = {S.ToString("F2", culture)}");
        }
        catch
        {
            Console.WriteLine("Loi!!");
        }
    }
}