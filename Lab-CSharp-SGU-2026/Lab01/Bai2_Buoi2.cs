using System;

class Bai2_Buoi2
{
    public static void Main (string[] agrs)
    {
        Console.Write("Moi ban nhap so thuc a: ");
        double a = double.Parse(Console.ReadLine());

        
        double[] F = new double[7];
        
        F[0] = a;                   
        F[1] = F[0] * F[0];         
        F[2] = F[1] * F[1];         
        F[3] = F[2] * F[0];        
        
        F[4] = F[2] * F[2];        
        F[5] = F[4] * F[4];         
        F[6] = F[5] * F[0];        

        Console.WriteLine($"Ket qua: {a}^2={F[1]:F2}, {a}^5={F[3]:F2}, {a}^17={F[6]:F2}.");
    
    }
}