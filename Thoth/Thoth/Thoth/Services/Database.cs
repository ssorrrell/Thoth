using System;
using SQLite;

namespace Thoth.Services
{
    public sealed class Database
    {
        private static readonly Lazy<Database>
            lazy = new Lazy<Database>(() => new Database());

        private static Database Instance { get { return lazy.Value; } }

        private static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        public static SQLiteAsyncConnection DbConnection => lazyInitializer.Value;
    }
}