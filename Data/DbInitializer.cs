public static class DbInitializer
{
    public static void Init(AppDbContext context, SeedSettings settings)
    {
        var users = context.Users;



        Console.WriteLine("\n\nGenerating Seed...\n\n");

        if (users.FirstOrDefault(u => u.Email == settings.Email) != null)
        {
            Console.WriteLine($"\n\nUser with email {settings.Email} already exists. Exiting...\n\n");
            return;
        }


        string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(settings.Password);

        User admin = new() { FirstName = settings.FirstName, LastName = settings.LastName, Email = settings.Email, Password = hashedPassword, Role = RoleE.ADMIN, IsActive = true };

        users.Add(admin);
        context.SaveChanges();
        Console.Write("\n\n");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"Successfully generated user:\nEmail: {settings.Email}\nPassword: {settings.Password}");
        Console.WriteLine(new string('-', 50));
        Console.Write("\n\n");
    }
}