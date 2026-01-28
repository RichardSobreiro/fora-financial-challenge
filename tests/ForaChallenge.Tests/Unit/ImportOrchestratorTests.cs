using FluentAssertions;
using ForaChallenge.Core.Enums;
using ForaChallenge.Core.Interfaces;
using ForaChallenge.Core.Models;
using ForaChallenge.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ForaChallenge.Tests.Unit;

public class ImportOrchestratorTests
{
    private readonly Mock<ICompanyRepository> _repositoryMock;
    private readonly Mock<IImportJobQueue> _queueMock;
    private readonly Mock<ILogger<ImportOrchestrator>> _loggerMock;
    private readonly ImportOrchestrator _orchestrator;

    public ImportOrchestratorTests()
    {
        _repositoryMock = new Mock<ICompanyRepository>();
        _queueMock = new Mock<IImportJobQueue>();
        _loggerMock = new Mock<ILogger<ImportOrchestrator>>();
        _orchestrator = new ImportOrchestrator(_repositoryMock.Object, _queueMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task StartImportAsync_CreatesJobAndEnqueuesWorkItem()
    {
        // Arrange
        var lockRecord = new ImportLock { Id = 1, CurrentJobId = null };
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.UpdateImportLockAsync(It.IsAny<ImportLock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveImportJobAsync(It.IsAny<ImportJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _queueMock
            .Setup(q => q.EnqueueAsync(It.IsAny<ImportWorkItem>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var jobId = await _orchestrator.StartImportAsync();

        // Assert
        jobId.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.SaveImportJobAsync(
            It.Is<ImportJob>(j => j.Status == ImportStatus.Queued && j.Id == jobId),
            It.IsAny<CancellationToken>()), Times.Once);
        _queueMock.Verify(q => q.EnqueueAsync(
            It.Is<ImportWorkItem>(w => w.JobId == jobId && !w.ForceReimport),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartImportAsync_WithForceReimport_PassesForceFlag()
    {
        // Arrange
        var lockRecord = new ImportLock { Id = 1, CurrentJobId = null };
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.UpdateImportLockAsync(It.IsAny<ImportLock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveImportJobAsync(It.IsAny<ImportJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _queueMock
            .Setup(q => q.EnqueueAsync(It.IsAny<ImportWorkItem>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _orchestrator.StartImportAsync(forceReimport: true);

        // Assert
        _queueMock.Verify(q => q.EnqueueAsync(
            It.Is<ImportWorkItem>(w => w.ForceReimport),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartImportAsync_WhenLockIsHeldByRunningJob_ThrowsException()
    {
        // Arrange
        var runningJobId = Guid.NewGuid();
        var lockRecord = new ImportLock
        {
            Id = 1,
            CurrentJobId = runningJobId
        };
        var runningJob = new ImportJob
        {
            Id = runningJobId,
            Status = ImportStatus.Running
        };

        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.GetImportJobAsync(runningJobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runningJob);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orchestrator.StartImportAsync());

        _queueMock.Verify(q => q.EnqueueAsync(It.IsAny<ImportWorkItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartImportAsync_WhenLockIsHeldByQueuedJob_ThrowsException()
    {
        // Arrange
        var queuedJobId = Guid.NewGuid();
        var lockRecord = new ImportLock
        {
            Id = 1,
            CurrentJobId = queuedJobId
        };
        var queuedJob = new ImportJob
        {
            Id = queuedJobId,
            Status = ImportStatus.Queued
        };

        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.GetImportJobAsync(queuedJobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queuedJob);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orchestrator.StartImportAsync());
    }

    [Fact]
    public async Task StartImportAsync_WhenLockIsHeldByCompletedJob_ClearsStaleLock()
    {
        // Arrange
        var completedJobId = Guid.NewGuid();
        var lockRecord = new ImportLock
        {
            Id = 1,
            CurrentJobId = completedJobId
        };
        var completedJob = new ImportJob
        {
            Id = completedJobId,
            Status = ImportStatus.Completed
        };

        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.GetImportJobAsync(completedJobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedJob);
        _repositoryMock
            .Setup(r => r.UpdateImportLockAsync(It.IsAny<ImportLock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveImportJobAsync(It.IsAny<ImportJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _queueMock
            .Setup(q => q.EnqueueAsync(It.IsAny<ImportWorkItem>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _orchestrator.StartImportAsync();

        // Assert
        _repositoryMock.Verify(r => r.UpdateImportLockAsync(
            It.Is<ImportLock>(l => l.CurrentJobId.HasValue && l.CurrentJobId != completedJobId),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartImportAsync_WhenEnqueueFails_MarksJobAsFailedAndUnlocks()
    {
        // Arrange
        var lockRecord = new ImportLock { Id = 1, CurrentJobId = null };
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);
        _repositoryMock
            .Setup(r => r.UpdateImportLockAsync(It.IsAny<ImportLock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveImportJobAsync(It.IsAny<ImportJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.GetImportJobAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportJob { Id = Guid.NewGuid(), Status = ImportStatus.Queued });
        _queueMock
            .Setup(q => q.EnqueueAsync(It.IsAny<ImportWorkItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Queue error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _orchestrator.StartImportAsync());

        _repositoryMock.Verify(r => r.UpdateImportJobAsync(
            It.Is<ImportJob>(j => j.Status == ImportStatus.Failed && !string.IsNullOrEmpty(j.ErrorMessage)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetImportStatusAsync_ReturnsJobFromRepository()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new ImportJob { Id = jobId, Status = ImportStatus.Running };
        _repositoryMock
            .Setup(r => r.GetImportJobAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await _orchestrator.GetImportStatusAsync(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(jobId);
        result.Status.Should().Be(ImportStatus.Running);
    }

    [Fact]
    public async Task GetCurrentImportAsync_ReturnsLatestJobFromRepository()
    {
        // Arrange
        var job = new ImportJob { Id = Guid.NewGuid(), Status = ImportStatus.Running };
        _repositoryMock
            .Setup(r => r.GetLatestImportJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await _orchestrator.GetCurrentImportAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ImportStatus.Running);
    }

    [Fact]
    public async Task IsImportRunningAsync_WhenLockHasJobId_ReturnsTrue()
    {
        // Arrange
        var lockRecord = new ImportLock
        {
            Id = 1,
            CurrentJobId = Guid.NewGuid()
        };
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);

        // Act
        var result = await _orchestrator.IsImportRunningAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsImportRunningAsync_WhenLockHasNoJobId_ReturnsFalse()
    {
        // Arrange
        var lockRecord = new ImportLock
        {
            Id = 1,
            CurrentJobId = null
        };
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockRecord);

        // Act
        var result = await _orchestrator.IsImportRunningAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsImportRunningAsync_WhenLockIsNull_ReturnsFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetImportLockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportLock?)null);

        // Act
        var result = await _orchestrator.IsImportRunningAsync();

        // Assert
        result.Should().BeFalse();
    }
}
