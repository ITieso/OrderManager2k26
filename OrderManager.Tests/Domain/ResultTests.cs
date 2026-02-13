using FluentAssertions;
using OrderManager.Domain.Common;

namespace OrderManager.Tests.Domain;

public class ResultTests
{
    /// <summary>
    /// Verifica se Result.Success() cria um resultado de sucesso sem erros.
    /// </summary>
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    /// <summary>
    /// Verifica se Result.Failure() cria um resultado de falha com o erro especificado.
    /// </summary>
    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Verifica se Result.Success<T>() carrega o valor corretamente.
    /// </summary>
    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    /// <summary>
    /// Verifica se acessar Value em resultado de falha lança exceção.
    /// </summary>
    [Fact]
    public void Failure_WithValue_ShouldNotAllowAccessToValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var act = () => result.Value;

        // Assert
        result.IsFailure.Should().BeTrue();
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifica se a conversão implícita de valor cria um resultado de sucesso.
    /// </summary>
    [Fact]
    public void ImplicitConversion_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
