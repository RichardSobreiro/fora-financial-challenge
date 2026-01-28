using FluentAssertions;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ForaChallenge.Tests.Unit;

public class CompanyImportProcessorTests
{
    private readonly Mock<ICompanyRepository> _repositoryMock;
    private readonly Mock<IEdgarDataProvider> _dataProviderMock;
    private readonly Mock<ILogger<CompanyImportProcessor>> _loggerMock;
    private readonly CompanyImportProcessor _processor;

    public CompanyImportProcessorTests()
    {
        _repositoryMock = new Mock<ICompanyRepository>();
        _dataProviderMock = new Mock<IEdgarDataProvider>();
        _loggerMock = new Mock<ILogger<CompanyImportProcessor>>();
        _processor = new CompanyImportProcessor(_repositoryMock.Object, _dataProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessCompanyAsync_WhenCompanyDoesNotExist_AddsNewCompany()
    {
        // Arrange
        const int cik = 12345;
        var companyData = new Company
        {
            Cik = cik,
            Name = "New Company",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2021, Income = 1_000_000 }
            }
        };

        _dataProviderMock
            .Setup(d => d.GetCompanyDataAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(companyData);
        _repositoryMock
            .Setup(r => r.GetCompanyByCikAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);
        _repositoryMock
            .Setup(r => r.AddCompanyAsync(companyData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessCompanyAsync(cik);

        // Assert
        _repositoryMock.Verify(r => r.AddCompanyAsync(companyData, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessCompanyAsync_WhenCompanyExists_UpdatesCompany()
    {
        // Arrange
        const int cik = 12345;
        var existingCompany = new Company
        {
            Id = 1,
            Cik = cik,
            Name = "Old Name",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2020, Income = 500_000 }
            }
        };

        var newCompanyData = new Company
        {
            Cik = cik,
            Name = "New Name",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2021, Income = 1_000_000 }
            }
        };

        _dataProviderMock
            .Setup(d => d.GetCompanyDataAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCompanyData);
        _repositoryMock
            .Setup(r => r.GetCompanyByCikAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCompany);
        _repositoryMock
            .Setup(r => r.UpdateCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessCompanyAsync(cik);

        // Assert
        existingCompany.Name.Should().Be("New Name");
        existingCompany.IncomeRecords.Should().HaveCount(2);
        existingCompany.IncomeRecords.Should().Contain(r => r.Year == 2020);
        existingCompany.IncomeRecords.Should().Contain(r => r.Year == 2021);

        _repositoryMock.Verify(r => r.UpdateCompanyAsync(existingCompany, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessCompanyAsync_WhenCompanyExists_UpdatesExistingIncomeRecord()
    {
        // Arrange
        const int cik = 12345;
        var existingRecord = new IncomeRecord { Year = 2021, Income = 500_000, Form = "10-K", Frame = "CY2021" };
        var existingCompany = new Company
        {
            Id = 1,
            Cik = cik,
            Name = "Test Corp",
            IncomeRecords = new List<IncomeRecord> { existingRecord }
        };

        var newCompanyData = new Company
        {
            Cik = cik,
            Name = "Test Corp",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2021, Income = 1_000_000, Form = "10-K", Frame = "CY2021" }
            }
        };

        _dataProviderMock
            .Setup(d => d.GetCompanyDataAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCompanyData);
        _repositoryMock
            .Setup(r => r.GetCompanyByCikAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCompany);
        _repositoryMock
            .Setup(r => r.UpdateCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessCompanyAsync(cik);

        // Assert
        existingCompany.IncomeRecords.Should().HaveCount(1);
        existingRecord.Income.Should().Be(1_000_000);
        existingRecord.Form.Should().Be("10-K");
        existingRecord.Frame.Should().Be("CY2021");
    }

    [Fact]
    public async Task ProcessCompanyAsync_WhenNoDataReturned_DoesNotAddOrUpdate()
    {
        // Arrange
        const int cik = 12345;
        _dataProviderMock
            .Setup(d => d.GetCompanyDataAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        await _processor.ProcessCompanyAsync(cik);

        // Assert
        _repositoryMock.Verify(r => r.GetCompanyByCikAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AddCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessCompanyAsync_WhenCompanyExists_AddsNewIncomeRecords()
    {
        // Arrange
        const int cik = 12345;
        var existingCompany = new Company
        {
            Id = 1,
            Cik = cik,
            Name = "Test Corp",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2020, Income = 500_000 }
            }
        };

        var newCompanyData = new Company
        {
            Cik = cik,
            Name = "Test Corp",
            IncomeRecords = new List<IncomeRecord>
            {
                new() { Year = 2020, Income = 500_000 },
                new() { Year = 2021, Income = 1_000_000 },
                new() { Year = 2022, Income = 1_500_000 }
            }
        };

        _dataProviderMock
            .Setup(d => d.GetCompanyDataAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCompanyData);
        _repositoryMock
            .Setup(r => r.GetCompanyByCikAsync(cik, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCompany);
        _repositoryMock
            .Setup(r => r.UpdateCompanyAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessCompanyAsync(cik);

        // Assert
        existingCompany.IncomeRecords.Should().HaveCount(3);
        existingCompany.IncomeRecords.Should().Contain(r => r.Year == 2020);
        existingCompany.IncomeRecords.Should().Contain(r => r.Year == 2021);
        existingCompany.IncomeRecords.Should().Contain(r => r.Year == 2022);
    }
}
