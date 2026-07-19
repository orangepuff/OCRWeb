using Diagnostics.Abstractions.Interfaces;
using Moq;

namespace OCRWeb.Identity.UnitTests;

/// <summary>Shared mock setup for handler tests.</summary>
internal static class TestHelpers
{
    public static Mock<ITransactionLogger> CreateTransactionLogger()
    {
        var scope = new Mock<ITransactionScope>();
        var logger = new Mock<ITransactionLogger>();
        logger.Setup(l => l.BeginTransaction(It.IsAny<string>(), It.IsAny<string?>())).Returns(scope.Object);
        return logger;
    }
}
