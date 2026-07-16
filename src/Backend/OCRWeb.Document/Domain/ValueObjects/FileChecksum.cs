using System.Security.Cryptography;

namespace OCRWeb.Document.Domain.ValueObjects;

/// <summary>
/// SHA-256 checksum of a file's binary content. Stored as VARBINARY(32).
/// </summary>
public sealed class FileChecksum : IEquatable<FileChecksum>
{
    public const int ByteLength = 32;

    public byte[] Value { get; }

    private FileChecksum(byte[] value) => Value = value;

    /// <summary>Compute from raw content.</summary>
    public static FileChecksum Compute(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return new FileChecksum(SHA256.HashData(content));
    }

    /// <summary>Rehydrate from a stored 32-byte value.</summary>
    public static FileChecksum FromStored(byte[] value)
    {
        if (value is null || value.Length != ByteLength)
            throw new ArgumentException($"Checksum must be {ByteLength} bytes.", nameof(value));
        return new FileChecksum(value);
    }

    public bool Equals(FileChecksum? other) => other is not null && Value.AsSpan().SequenceEqual(other.Value);
    public override bool Equals(object? obj) => obj is FileChecksum other && Equals(other);
    public override int GetHashCode() => Convert.ToHexStringLower(Value).GetHashCode();
}
