using Dapper;
using DapperIntro.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System;
using System.Net.NetworkInformation;

namespace DapperIntro;

public class Program
{
    public static void Main(string[] args)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "library.db");
        string connectionString = $"Data Source={path}";
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        InitDatabase(connection);

        //Book b = new Book()
        //{
        //    Title = "Dune",
        //    Author = "Frank Herbert"
        //};

        //AddBook(connection, b);
        ReadAllBooks(connection);

        string filterQuery = "SELECT * FROM Books WHERE Title LIKE 'D%'";

        List<Book> booksD = connection.Query<Book>(filterQuery).ToList();

        foreach(Book book in booksD)
        {
            Console.WriteLine(book);
        }

        string findQuery = @"SELECT * FROM Books WHERE Id = @Id;";

        Book? bookWithId = connection.QueryFirstOrDefault<Book>(findQuery, new Book { Id = 1 });

        Console.WriteLine(bookWithId);

        if(bookWithId != null )
        {
            bookWithId.Title = "Sherlock Holmes";
            bookWithId.Author = "Arthur Conan Doyle";

            string updateQuery = @"
                UPDATE Books
                SET Author = @Author, Title = @Title
                WHERE Id = @Id;
            ";

            connection.Execute(updateQuery, bookWithId);
        }

        ReadAllBooks(connection);

        Book? bookWithIdToDelete = connection.QueryFirstOrDefault<Book>(findQuery, new Book { Id = 2 });

        if (bookWithIdToDelete != null)
        {
            connection.Execute("DELETE FROM Books WHERE Id = @Id", bookWithIdToDelete);
        }
        else
        {
            Console.WriteLine("No such book in DB");
        }

        ReadAllBooks(connection);

        int bookCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Books;");

        //Book? top1 = connection.QueryFirstOrDefault<Book>("SELECT * FROM Books LIMIT 1");

        // Multiple queries
        var multiQuery = connection.QueryMultiple(@"
            SELECT * FROM Books;
            SELECT COUNT(*) FROM Books;
        ");

        var allBooks = multiQuery.Read<Book>().ToList();
        var totalBooks = multiQuery.ReadSingle<int>();

        Console.WriteLine($"Books count: {bookCount}");
    }

    private static void AddBook(SqliteConnection connection, Book b)
    {
        string insertQuery = "INSERT INTO Books(Title, Author) VALUES (@Title, @Author);";

        connection.Execute(insertQuery, b);
    }

    private static void ReadAllBooks(SqliteConnection connection)
    {
        string selectQuery = "SELECT * FROM Books;";

        var books = connection.Query<Book>(selectQuery).ToList();

        foreach (var book in books)
        {
            Console.WriteLine(book);
        }
    }

    private static void InitDatabase(SqliteConnection connection)
    {
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Books (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT,
                Author TEXT);
        ");
    }
}