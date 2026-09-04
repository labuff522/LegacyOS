using Npgsql;

namespace LegacyOS.Api.Infrastructure;

public static class RenderDatabaseUrl
{
    public static string? ToNpgsqlConnectionString(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl)) return null;
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
            throw new InvalidOperationException("DATABASE_URL must be a valid PostgreSQL URL.");

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2)
            throw new InvalidOperationException("DATABASE_URL must include a username and password.");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1]),
            SslMode = SslMode.Prefer
        };
        return builder.ConnectionString;
    }
}
