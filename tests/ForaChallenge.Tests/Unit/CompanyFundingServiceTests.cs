using FluentAssertions;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Moq;
using Xunit;

namespace ForaChallenge.Tests.Unit;

public class CompanyFundingServiceTests
{
    private readonly Mock<ICompanyRepository> _repositoryMock;
    private readonly Mock<IFundingCalculator> _calculatorMock;
    private readonly CompanyFundingService _service;

    public CompanyFundingServiceTests()
    {
        _repositoryMock = new Mock<ICompanyRepository>();
        _calculatorMock = new Mock<IFundingCalculator>();
        _service = new CompanyFundingService(_repositoryMock.Object, _calculatorMock.Object);
    }

    [Fact]
    public async Task GetCompaniesWithFundingAsync_ReturnsOrderedResults()
    {
        // Arrange
        var companies = new List<Company>
        {
            new() { Id = 3, Name = "Company C" },
            new() { Id = 1, Name = "Company A" },
            new() { Id = 2, Name = "Company B" }
        };

        _repositoryMock
            .Setup(r => r.GetCompaniesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        _calculatorMock
            .Setup(c => c.CalculateStandardFunding(It.IsAny<Company>()))
            .Returns(1000m);
        _calculatorMock
            .Setup(c => c.CalculateSpecialFunding(It.IsAny<Company>(), 1000m))
            .Returns(1200m);

        // Act
        var results = await _service.GetCompaniesWithFundingAsync();

        // Assert
        results.Should().HaveCount(3);
        results[0].Id.Should().Be(1);
        results[1].Id.Should().Be(2);
        results[2].Id.Should().Be(3);
    }

    [Fact]
    public async Task GetCompaniesWithFundingAsync_WithNameFilter_PassesFilterToRepository()
    {
        // Arrange
        var companies = new List<Company> { new() { Id = 1, Name = "Apple Inc." } };
        _repositoryMock
            .Setup(r => r.GetCompaniesAsync("App", It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        _calculatorMock
            .Setup(c => c.CalculateStandardFunding(It.IsAny<Company>()))
            .Returns(1000m);
        _calculatorMock
            .Setup(c => c.CalculateSpecialFunding(It.IsAny<Company>(), 1000m))
            .Returns(1200m);

        // Act
        await _service.GetCompaniesWithFundingAsync("App");

        // Assert
        _repositoryMock.Verify(r => r.GetCompaniesAsync("App", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompaniesWithFundingAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetCompaniesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Company>());

        // Act
        var results = await _service.GetCompaniesWithFundingAsync();

        // Assert
        results.Should().BeEmpty();
        _calculatorMock.Verify(c => c.CalculateStandardFunding(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public void CalculateFunding_CallsCalculatorAndMapsResult()
    {
        // Arrange
        var company = new Company
        {
            Id = 1,
            Name = "Test Corp",
            IncomeRecords = new List<IncomeRecord>()
        };

        _calculatorMock
            .Setup(c => c.CalculateStandardFunding(company))
            .Returns(1000m);
        _calculatorMock
            .Setup(c => c.CalculateSpecialFunding(company, 1000m))
            .Returns(1200m);

        // Act
        var result = _service.CalculateFunding(company);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Corp");
        result.StandardFundableAmount.Should().Be(1000m);
        result.SpecialFundableAmount.Should().Be(1200m);

        _calculatorMock.Verify(c => c.CalculateStandardFunding(company), Times.Once);
        _calculatorMock.Verify(c => c.CalculateSpecialFunding(company, 1000m), Times.Once);
    }

    [Fact]
    public async Task GetCompaniesWithFundingAsync_RespectsCancellationToken()
    {
        // Arrange - simulate repository honoring cancellation
        _repositoryMock
            .Setup(r => r.GetCompaniesAsync(null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert - service should propagate the cancellation exception
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _service.GetCompaniesWithFundingAsync());
    }
}
