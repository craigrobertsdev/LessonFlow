using LessonFlow.Api.Database;
using LessonFlow.Api.Services;
using LessonFlow.Domain.ValueObjects;
using LessonFlow.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LessonFlow.UnitTests.Services;
public class TermDatesServiceTests
{
    private readonly ITermDatesService _termDatesService;

    public TermDatesServiceTests()
    {
         _termDatesService = CreateTermDatesService();
    }

    [Fact]
    public void GetTermNumber_DateInTerm1_Returns1()
    {
        // Arrange
        var date = new DateOnly(2025, 2, 15); // Date in Term 1
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(1, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateInTerm2_Returns2()
    {
        var date = new DateOnly(2025, 5, 10); // Date in Term 2
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(2, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateInBetweenTerms2And3_Returns3()
    {
        var date = new DateOnly(2025, 7, 22); // Date between Term 2 and Term 3
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(3, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateBeforeTerm1_Returns1()
    {
        var date = new DateOnly(2025, 1, 1); // Date before Term 1
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(1, termNumber);
    }

    [Fact]
    public void GetTermNumber_DateAfterTerm4_Returns4()
    {
        var date = new DateOnly(2025, 12, 31); // Date after Term 4
        // Act
        var termNumber = _termDatesService.GetTermNumber(date);
        // Assert
        Assert.Equal(4, termNumber);
    }

    private ITermDatesService CreateTermDatesService()
    {
        var termDatesByYear = new Dictionary<int, List<SchoolTerm>>()
        {
            {
            2025,
            [
                new SchoolTerm(1, new DateOnly(2025, 1, 27), new DateOnly(2025, 4,11)),
                new SchoolTerm(2, new DateOnly(2025, 4, 28), new DateOnly(2025, 7, 4)),
                new SchoolTerm(3, new DateOnly(2025, 7, 21), new DateOnly(2025, 9, 26)),
                new SchoolTerm(4, new DateOnly(2025, 10, 13), new DateOnly(2025, 12, 22))
            ]
            }
        };

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new Mock<ApplicationDbContext>(dbContextOptions);

        var mockDbSet = new Mock<DbSet<SchoolTerm>>();
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.Provider).Returns(termDatesByYear[2025].AsQueryable().Provider);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.Expression).Returns(termDatesByYear[2025].AsQueryable().Expression);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.ElementType).Returns(termDatesByYear[2025].AsQueryable().ElementType);
        mockDbSet.As<IQueryable<SchoolTerm>>().Setup(m => m.GetEnumerator()).Returns(termDatesByYear[2025].AsQueryable().GetEnumerator());

        dbContext.Setup(db => db.TermDates).Returns(mockDbSet.Object);
        // Mock the IServiceScope
        var mockScope = new Mock<IServiceScope>();
        var mockScopedServiceProvider = new Mock<IServiceProvider>();

        mockScopedServiceProvider
            .Setup(sp => sp.GetService(typeof(ApplicationDbContext)))
            .Returns(dbContext.Object);

        mockScope
            .Setup(s => s.ServiceProvider)
            .Returns(mockScopedServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(mockScope.Object);

        var mockRootServiceProvider = new Mock<IServiceProvider>();
        mockRootServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        var service = new TermDatesService(mockRootServiceProvider.Object);

        foreach (var (year, terms) in termDatesByYear)
        {
            service.SetTermDates(year, terms);
        }
        return service;
    }
}
