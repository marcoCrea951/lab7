using LibraryManagementBlazor.Models;
using LibraryManagementBlazor.Services;
using NUnit.Framework;

namespace LibraryManagementBlazor.Tests;

[TestFixture]
[NonParallelizable]
public class LibraryServiceTests
{
    private string oldFolder = "";
    private string testFolder = "";
    private LibraryService library = null!;

    [SetUp]
    public void SetUp()
    {
        // arrange, use a separate folder so the real CSV files are not changed.
        oldFolder = Environment.CurrentDirectory;
        testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testFolder);
        Environment.CurrentDirectory = testFolder;

        library = new LibraryService();
    }

    [TearDown]
    public void TearDown()
    {
        Environment.CurrentDirectory = oldFolder;

        if (Directory.Exists(testFolder))
        {
            Directory.Delete(testFolder, true);
        }
    }

    [Test]
    public void ReadBooks_GetsBooksFromFile()
    {
        // arrange
        File.WriteAllText("Data/Books.csv",
            "Id,Title,Author\n1,Dune,Frank Herbert\n2,The Hobbit,J.R.R. Tolkien");

        // act
        library.ReadBooks();

        // Assert
        Assert.That(library.Books.Count, Is.EqualTo(2));
        Assert.That(library.Books[0].Title, Is.EqualTo("Dune"));
    }

    [Test]
    public void ReadBooks_SkipsRowsWithBadIds()
    {
        // Arrange
        File.WriteAllText("Data/Books.csv",
            "Id,Title,Author\nabc,Wrong Book,Some Author\n3,Good Book,Someone");

        
        library.ReadBooks();

        
        Assert.That(library.Books.Count, Is.EqualTo(1));
        Assert.That(library.Books[0].Id, Is.EqualTo(3));
    }

    [Test]
    public void ReadUsers_WhenFileIsMissing_LeavesListEmpty()
    {
        // arrange: no Users.csv file was created

        // Act
        library.ReadUsers();

        // Assert
        Assert.That(library.Users, Is.Empty);
    }

    [Test]
    public void AddBook_GivesBookANewId()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Dune", Author = "Frank Herbert" });
        Book book = new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien" };

        // Act
        library.AddBook(book);

        // Assert
        Assert.That(book.Id, Is.EqualTo(2));
        Assert.That(library.Books.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddBook_UsesOneWhenThereAreNoBooks()
    {
        // Arrange
        Book book = new Book { Title = "Dune", Author = "Frank Herbert" };

        // Act
        library.AddBook(book);

        // Assert
        Assert.That(book.Id, Is.EqualTo(1));
    }

    [Test]
    public void EditBook_ChangesTheBookInformation()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Old Title", Author = "Old Author" });
        Book changedBook = new Book { Id = 1, Title = "New Title", Author = "New Author" };

        // Act
        library.EditBook(changedBook);

        // Assert
        Assert.That(library.Books[0].Title, Is.EqualTo("New Title"));
        Assert.That(library.Books[0].Author, Is.EqualTo("New Author"));
    }

    [Test]
    public void EditBook_DoesNothingIfBookIsNotFound()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Dune", Author = "Frank Herbert" });
        Book changedBook = new Book { Id = 5, Title = "New Title", Author = "New Author" };

        // act
        library.EditBook(changedBook);

        // assert
        Assert.That(library.Books.Count, Is.EqualTo(1));
        Assert.That(library.Books[0].Title, Is.EqualTo("Dune"));
    }

    [Test]
    public void DeleteBook_RemovesBookFromList()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Dune", Author = "Frank Herbert" });

        // Act
        library.DeleteBook(1);

        // Assert
        Assert.That(library.Books, Is.Empty);
    }

    [Test]
    public void AddUser_AddsUserAndAssignsId()
    {
        // Arrange
        User user = new User { Name = "Marco" };

        // Act
        library.AddUser(user);

        // Assert
        Assert.That(user.Id, Is.EqualTo(1));
        Assert.That(library.Users.Count, Is.EqualTo(1));
    }

    [Test]
    public void EditUser_ChangesUserName()
    {
        // Arrange
        library.AddUser(new User { Id = 1, Name = "Alex" });
        User changedUser = new User { Id = 1, Name = "Jordan" };

        // Act
        library.EditUser(changedUser);

        // Assert
        Assert.That(library.Users[0].Name, Is.EqualTo("Jordan"));
    }

    [Test]
    public void DeleteUser_RemovesUser()
    {
        // Arrange
        library.AddUser(new User { Id = 1, Name = "Alex" });

        // Act
        library.DeleteUser(1);

        // Assert
        Assert.That(library.Users, Is.Empty);
    }

    [Test]
    public void BorrowBook_WorksWhenBookAndUserExist()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Dune", Author = "Frank Herbert" });
        library.AddUser(new User { Id = 10, Name = "Alex" });

        // Act
        bool borrowed = library.BorrowBook(10, 1);

        // Assert
        Assert.That(borrowed, Is.True);
        Assert.That(library.BorrowedBooks[1], Is.EqualTo(10));
    }

    [Test]
    public void BorrowBook_FailsIfBookDoesNotExist()
    {
        // Arrange
        library.AddUser(new User { Id = 10, Name = "Alex" });

        // Act
        bool borrowed = library.BorrowBook(10, 99);

        // Assert
        Assert.That(borrowed, Is.False);
    }

    [Test]
    public void BorrowBook_FailsIfBookIsAlreadyBorrowed()
    {
        // Arrange
        library.AddBook(new Book { Id = 1, Title = "Dune", Author = "Frank Herbert" });
        library.AddUser(new User { Id = 10, Name = "Alex" });
        library.AddUser(new User { Id = 20, Name = "Jordan" });
        library.BorrowBook(10, 1);

        // Act
        bool borrowedAgain = library.BorrowBook(20, 1);

        // Assert
        Assert.That(borrowedAgain, Is.False);
        Assert.That(library.BorrowedBooks[1], Is.EqualTo(10));
    }

    [Test]
    public void ReturnBook_RemovesBorrowedBook()
    {
        // Arrange
        library.BorrowedBooks.Add(1, 10);

        // Act
        library.ReturnBook(1);

        // Assert
        Assert.That(library.BorrowedBooks.ContainsKey(1), Is.False);
    }
}
