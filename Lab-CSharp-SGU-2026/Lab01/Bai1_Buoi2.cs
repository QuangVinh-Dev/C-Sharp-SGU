using System;

class Bai1_Buoi2
{
    public static void Main(string[] args)
    {
        Console.Write("Moi ban nhap so a ");
        long a=long.Parse(Console.ReadLine());

        Console.Write("Moi ban nhap so b ");
        long b=long.Parse(Console.ReadLine());

        long Tong1DenB= b*(b+1) /2;

        long aDen1= a-1;
        long Tong1DenATru1=aDen1*(aDen1+1)/2;

        long ketQua = Tong1DenB - Tong1DenATru1;

        Console.WriteLine($"Tong cua cac so trong doan[{a}, {b}] la {ketQua}.");

    }
}