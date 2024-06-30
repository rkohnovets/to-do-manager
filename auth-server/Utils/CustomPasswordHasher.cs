namespace auth_server.Utils;

public static class CustomPasswordHasher
{
    public static string Hash(string password)
    {
        // TODO: сделать более правильное хеширование
        //return password.GetHashCode().ToString();
        return password;
    }
}
