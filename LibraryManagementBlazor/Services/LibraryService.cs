namespace LibraryManagementBlazor.Services;
using LibraryManagementBlazor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LibraryService
{
    private readonly string booksFilePath = "Data/Books.csv";
    private readonly string usersFilePath = "Data/Users.csv";

    public List<Book> Books { get; private set; } = new();
    public List<User> Users { get; private set; } = new();
    
    // Key: BookId, Value: UserId (Tracks borrowed books)
    public Dictionary<int, int> BorrowedBooks { get; private set; } = new();

    public LibraryService()
    {
    
        Directory.CreateDirectory("Data");

        ReadBooks();
        ReadUsers();
    }

    //  CSV Reading Methods 

    public void ReadBooks()
    {
        Books.Clear();
        if (File.Exists(booksFilePath))
        {
            var lines = File.ReadAllLines(booksFilePath);
            foreach (var line in lines.Skip(1)) // Skip header row
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int id))
                    {
                        Books.Add(new Book
                        {
                            Id = id,
                            Title = parts[1].Trim(),
                            Author = parts[2].Trim()
                        });
                    }
                }
            }
        }
    }

    public void ReadUsers()
    {
        Users.Clear();
        if (File.Exists(usersFilePath))
        {
            var lines = File.ReadAllLines(usersFilePath);
            foreach (var line in lines.Skip(1)) // Skip header row
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[0], out int id))
                    {
                        Users.Add(new User
                        {
                            Id = id,
                            Name = parts[1].Trim()
                        });
                    }
                }
            }
        }
    }

    // CSV Writing Methods (Persistence)

    private void WriteBooks()
    {
        var lines = new List<string> { "Id,Title,Author" }; // Header
        foreach (var book in Books)
        {
            lines.Add($"{book.Id},{book.Title},{book.Author}");
        }
        File.WriteAllLines(booksFilePath, lines);
    }

    private void WriteUsers()
    {
        var lines = new List<string> { "Id,Name" }; // Header
        foreach (var user in Users)
        {
            lines.Add($"{user.Id},{user.Name}");
        }
        File.WriteAllLines(usersFilePath, lines);
    }

    // Book CRUD Operations

    public void AddBook(Book book)
    {
        // Auto-generate ID if not set
        if (book.Id == 0)
        {
            book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
        }
        Books.Add(book);
        WriteBooks();
    }

    public void EditBook(Book updatedBook)
    {
        var book = Books.FirstOrDefault(b => b.Id == updatedBook.Id);
        if (book != null)
        {
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            WriteBooks();
        }
    }

    public void DeleteBook(int bookId)
    {
        Books.RemoveAll(b => b.Id == bookId);
        BorrowedBooks.Remove(bookId); // Clean up borrow state if deleted
        WriteBooks();
    }

    //  User CRUD Operations 

    public void AddUser(User user)
    {
        if (user.Id == 0)
        {
            user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
        }
        Users.Add(user);
        WriteUsers();
    }

    public void EditUser(User updatedUser)
    {
        var user = Users.FirstOrDefault(u => u.Id == updatedUser.Id);
        if (user != null)
        {
            user.Name = updatedUser.Name;
            WriteUsers();
        }
    }

    public void DeleteUser(int userId)
    {
        Users.RemoveAll(u => u.Id == userId);
        
        // Remove any books borrowed by this user
        var borrowedByThisUser = BorrowedBooks.Where(kvp => kvp.Value == userId).Select(kvp => kvp.Key).ToList();
        foreach (var bookId in borrowedByThisUser)
        {
            BorrowedBooks.Remove(bookId);
        }

        WriteUsers();
    }

    // Borrowing 

    public bool BorrowBook(int userId, int bookId)
    {
        // Check if book exists and is not already borrowed
        var bookExists = Books.Any(b => b.Id == bookId);
        var userExists = Users.Any(u => u.Id == userId);

        if (bookExists && userExists && !BorrowedBooks.ContainsKey(bookId))
        {
            BorrowedBooks[bookId] = userId;
            return true;
        }
        return false;
    }

    public void ReturnBook(int bookId)
    {
        BorrowedBooks.Remove(bookId);
    }
}