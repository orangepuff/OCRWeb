using Microsoft.Data.SqlClient;

namespace OCRWeb.Document.Infrastructure.Repositories;

/// <summary>
/// Wraps the stream returned by <see cref="SqlDataReader.GetStream"/> so the owning
/// reader/command/connection are disposed together with it, instead of being left open
/// until the caller separately remembers to dispose three other objects.
/// </summary>
internal sealed class SqlBlobStream(
    SqlConnection connection, SqlCommand command, SqlDataReader reader, Stream inner) : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        inner.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        inner.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        inner.ReadAsync(buffer, cancellationToken);

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            inner.Dispose();
            reader.Dispose();
            command.Dispose();
            connection.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await inner.DisposeAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        await connection.DisposeAsync();
    }
}
