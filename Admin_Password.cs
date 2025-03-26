using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "@dmin_role123";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine(hashedPassword);
    }
}
