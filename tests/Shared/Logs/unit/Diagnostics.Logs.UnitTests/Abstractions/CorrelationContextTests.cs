using Diagnostics.Abstractions;

namespace Diagnostics.Logs.UnitTests.Abstractions;

public class CorrelationContextTests
{
    [Fact]
    public void CorrelationId_WhenNeverSet_IsStableAcrossRepeatedReads()
    {
        var sut = new CorrelationContext();

        var first = sut.CorrelationId;
        var second = sut.CorrelationId;

        // Regression guard: the getter used to mint a brand new guid on every read when nothing
        // had called SetCorrelationId, which would silently break correlation for any code path
        // that reads CorrelationId more than once outside of an HTTP request (e.g. background jobs).
        Assert.Equal(first, second);
    }

    [Fact]
    public void SetCorrelationId_ThenCorrelationId_ReturnsTheSetValue()
    {
        var sut = new CorrelationContext();
        var expected = Guid.NewGuid();

        sut.SetCorrelationId(expected);

        Assert.Equal(expected, sut.CorrelationId);
    }

    [Fact]
    public void CurrentCategory_WhenNoTransactionOpen_IsNoneGuardrail()
    {
        var sut = new CorrelationContext();

        Assert.Equal(CategoryNames.None, sut.CurrentCategory);
    }

    [Fact]
    public void CurrentTransactionId_WhenNoTransactionOpen_IsNull()
    {
        var sut = new CorrelationContext();

        Assert.Null(sut.CurrentTransactionId);
        Assert.Null(sut.CurrentParentTransactionId);
    }

    [Fact]
    public void PushTransaction_SetsAmbientTransactionAndCategory()
    {
        var sut = new CorrelationContext();
        var transactionId = Guid.NewGuid();

        using (sut.PushTransaction(transactionId, parentTransactionId: null, category: "UploadPdf"))
        {
            Assert.Equal(transactionId, sut.CurrentTransactionId);
            Assert.Null(sut.CurrentParentTransactionId);
            Assert.Equal("UploadPdf", sut.CurrentCategory);
        }
    }

    [Fact]
    public void PushTransaction_Dispose_RestoresPreviousAmbientState()
    {
        var sut = new CorrelationContext();
        var outerId = Guid.NewGuid();

        using (sut.PushTransaction(outerId, null, "Outer"))
        {
            Assert.Equal(outerId, sut.CurrentTransactionId);

            using (sut.PushTransaction(Guid.NewGuid(), outerId, "Inner"))
            {
                Assert.Equal("Inner", sut.CurrentCategory);
            }

            // Disposing the inner scope must restore the outer scope's state, not clear it.
            Assert.Equal(outerId, sut.CurrentTransactionId);
            Assert.Equal("Outer", sut.CurrentCategory);
        }

        // Disposing the outer scope restores the "no transaction open" state.
        Assert.Null(sut.CurrentTransactionId);
        Assert.Equal(CategoryNames.None, sut.CurrentCategory);
    }

    [Fact]
    public void PushTransaction_NestedScope_CapturesEnclosingScopeAsParent()
    {
        var sut = new CorrelationContext();
        var outerId = Guid.NewGuid();

        using (sut.PushTransaction(outerId, null, "Outer"))
        {
            // Mirrors how TransactionScopeImpl derives ParentId from CurrentTransactionId
            // before opening the child scope.
            var parentId = sut.CurrentTransactionId;
            var innerId = Guid.NewGuid();

            using (sut.PushTransaction(innerId, parentId, "Inner"))
            {
                Assert.Equal(outerId, sut.CurrentParentTransactionId);
                Assert.Equal(innerId, sut.CurrentTransactionId);
            }
        }
    }

    [Fact]
    public void PushTransaction_PreservesCorrelationId_SetBeforeTheScopeOpened()
    {
        var sut = new CorrelationContext();
        var correlationId = Guid.NewGuid();
        sut.SetCorrelationId(correlationId);

        using (sut.PushTransaction(Guid.NewGuid(), null, "SomeCategory"))
        {
            Assert.Equal(correlationId, sut.CorrelationId);
        }

        Assert.Equal(correlationId, sut.CorrelationId);
    }

    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        var sut = new CorrelationContext();
        var scope = sut.PushTransaction(Guid.NewGuid(), null, "Category");

        scope.Dispose();
        var exception = Record.Exception(() => scope.Dispose());

        Assert.Null(exception);
    }
}
