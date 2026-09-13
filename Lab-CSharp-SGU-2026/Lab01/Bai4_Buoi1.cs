using System;

class Program
{
    static void Main(string[] args)
    {
        //Nhập dữ liệu
        Console.WriteLine("Nhập số giây:");
        int totalSeconds = int.Parse(Console.ReadLine());
        // Khai báo biến và xử lý dữ liệu     
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        // Xuất kết quả
        Console.WriteLine($"Kết quả: {hours} giờ, {minutes} phút, {seconds} giây");
    }
}
