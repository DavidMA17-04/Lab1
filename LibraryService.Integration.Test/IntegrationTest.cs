using System.Net;
using System.Text;
using FluentAssertions;
using LibraryService.Api;
using LibraryService.Api.Common.Persistence;
using LibraryService.Api.Features.Books;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace LibraryService.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly LibraryContext _context;

    public HttpClient Client { get; private set; }

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _context = new LibraryContext(new DbContextOptionsBuilder<LibraryContext>()
            .UseSqlite("DataSource=:memory:")
            .EnableSensitiveDataLogging()
            .Options);

        Client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(LibraryContext) ||
                    d.ServiceType == typeof(DbContextOptions<LibraryContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(LibraryContext))).ToList();

                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                services.AddSingleton(_context);

                _context.Database.OpenConnection();
                _context.Database.EnsureCreated();
                _context.SaveChanges();

                foreach (var entity in _context.ChangeTracker.Entries().ToList())
                {
                    entity.State = EntityState.Detached;
                }
            });
        }).CreateClient();
    }

    private async Task SeedLibrary()
    {
        var libraries = new List<Library>
        {
            new Library { Name = "Library Name 1", Location = "Location 1" },
            new Library { Name = "Library Name 2", Location = "Location 2" },
            new Library { Name = "Library Name 3", Location = "Location 3" },
            new Library { Name = "Library Name 4", Location = "Location 4" }
        };

        await _context.Libraries.AddRangeAsync(libraries);
        await _context.SaveChangesAsync();
    }

    private async Task SeedBook(string bookName, int libraryId)
    {
        var bookForm = new BookForm
        {
            Name = bookName,
            Category = "General"
        };
        await Client.PostAsync($"/api/libraries/{libraryId}/books",
            new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
    }

    [Fact]
    public async Task TestAddBook_Ok_GetBook_NotFound()
    {
        await SeedLibrary();

        var bookForm = new BookForm { Name = "Test book 1", Category = "Fiction" };
        var response1 = await Client.PostAsync("/api/libraries/1/books",
            new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        bookForm = new BookForm { Name = "Test book 2", Category = "Fiction" };
        var response2 = await Client.PostAsync("/api/libraries/100/books",
            new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestGetBooks_Ok_NotFound()
    {
        await SeedLibrary();
        await SeedBook("test book 1", 1);
        await SeedBook("test book 2", 1);

        var response1 = await Client.GetAsync("/api/libraries/2/books");
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = JsonConvert.DeserializeObject<IEnumerable<Book>>(await response1.Content.ReadAsStringAsync())!.ToList();
        books.Count.Should().Be(0);

        var response2 = await Client.GetAsync("/api/libraries/1/books");
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var books2 = JsonConvert.DeserializeObject<IEnumerable<Book>>(await response2.Content.ReadAsStringAsync())!.ToList();
        books2.Count.Should().Be(2);

        var response3 = await Client.GetAsync("/api/libraries/31232/books");
        response3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestDeleteLibrary()
    {
        await SeedLibrary();

        var bookForm = new BookForm { Name = "test book 1", Category = "General" };
        var response0 = await Client.PostAsync("/api/libraries/1/books",
            new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        response0.StatusCode.Should().Be(HttpStatusCode.Created);

        var response1 = await Client.DeleteAsync("/api/libraries/1");
        response1.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response2 = await Client.GetAsync("/api/libraries/1/books");
        response2.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response3 = await Client.DeleteAsync("/api/libraries/1");
        response3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestUpdateAndDeleteBook()
    {
        await SeedLibrary();

        var bookForm = new BookForm { Name = "Original", Category = "A" };
        var createResponse = await Client.PostAsync("/api/libraries/1/books",
            new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = JsonConvert.DeserializeObject<Book>(await createResponse.Content.ReadAsStringAsync())!;

        var updateForm = new BookForm { Name = "Updated", Category = "B" };
        var updateResponse = await Client.PutAsync($"/api/libraries/1/books/{created.Id}",
            new StringContent(JsonConvert.SerializeObject(updateForm), Encoding.UTF8, "application/json"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteResponse = await Client.DeleteAsync($"/api/libraries/1/books/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var missingDelete = await Client.DeleteAsync("/api/libraries/999/books/1");
        missingDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
