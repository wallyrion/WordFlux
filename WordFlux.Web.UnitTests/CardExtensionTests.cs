using System;
using Xunit;
using FluentAssertions;
using WordFlux.Web.UnitTests.Factories;

public class CardExtensionTests
{
    [Fact]
    public void GetProgressRate_ShouldReturn0_WhenReviewIntervalIsLessThanOrEqualToMinInterval()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromSeconds(30)); // Less than 1 minute

        // Act
        double progressRate = card.GetProgressRate();

        // Assert
        progressRate.Should().Be(0);
    }

    [Fact]
    public void GetProgressRate_ShouldReturn100_WhenReviewIntervalIsGreaterThanOrEqualToMaxInterval()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromDays(90)); // 3 months

        // Act
        double progressRate = card.GetProgressRate();

        // Assert
        progressRate.Should().Be(100);
    }

    [Fact]
    public void GetProgressRate_ShouldReturnCorrectProgress_ForMiddleInterval()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromDays(15)); // Midway between the initial and final interval

        // Act
        double progressRate = card.GetProgressRate();

        // Assert
        progressRate.Should().BeGreaterThan(0).And.BeLessThan(100); // Should be somewhere in between
    }

    [Fact]
    public void GetProgressRate_ShouldReturn20Percent_WhenReviewIntervalIs1Day()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromDays(1)); // 1 day

        // Act
        double progressRate = card.GetProgressRate();

        // Assert
        progressRate.Should().BeApproximately(20, 0.1); // Expecting around 20%
    }

    [Fact]
    public void GetProgressRate_ShouldReturn90Percent_WhenReviewIntervalIs64Days()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromDays(64)); // 64 days

        // Act
        double progressRate = card.GetProgressRate();

        // Assert
        progressRate.Should().BeApproximately(90, 0.1); // Expecting around 90%
    }

    [Fact]
    public void GetProgressRate_ShouldReturnProgress_IncreasingAsIntervalsDouble()
    {
        // Arrange
        var card = CardFactory.Create(TimeSpan.FromMinutes(2)); // First approval
        var progressRate1 = card.GetProgressRate();

        card = CardFactory.Create(TimeSpan.FromMinutes(4)); // Second approval
        var progressRate2 = card.GetProgressRate();

        card = CardFactory.Create(TimeSpan.FromMinutes(8)); // Third approval
        var progressRate3 = card.GetProgressRate();

        // Assert
        progressRate2.Should().BeGreaterThan(progressRate1); // Progress should increase
        progressRate3.Should().BeGreaterThan(progressRate2); // Progress should continue increasing
    }
}
