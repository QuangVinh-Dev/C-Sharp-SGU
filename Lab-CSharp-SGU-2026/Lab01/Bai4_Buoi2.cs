using System;

class Program
{
    static void Main(string[] args)
    {
       int thang;
         Console.WriteLine("Nhập số tháng (1-12):");
          thang = int.Parse(Console.ReadLine());
         int quy=(thang-1)/3+1;
         Console.WriteLine("Tháng {0} thuộc quý {1}", thang, quy);
          
    }
 }  


