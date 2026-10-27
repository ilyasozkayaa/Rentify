namespace RentifyApi.Observability;

internal sealed class ResponseCaptureStream(Stream destination, int maximumBytes, Func<bool> isSafeToCapture) : Stream
{
    private MemoryStream? _capture;
    private bool? _shouldCapture;
    private bool _disposed;

    public bool IsTruncated { get; private set; }
    public ReadOnlySpan<byte> CapturedBytes => _capture is null ? default : _capture.GetBuffer().AsSpan(0, (int)_capture.Length);

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => destination.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() => destination.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => destination.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        destination.Write(buffer, offset, count);
        Capture(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        destination.Write(buffer);
        Capture(buffer);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await destination.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        Capture(buffer.AsSpan(offset, count));
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await destination.WriteAsync(buffer, cancellationToken);
        Capture(buffer.Span);
    }

    private void Capture(ReadOnlySpan<byte> bytes)
    {
        if (_disposed || bytes.IsEmpty)
            return;

        try
        {
            _shouldCapture ??= isSafeToCapture();

            if (_shouldCapture != true)
                return;

            _capture ??= new MemoryStream(Math.Min(maximumBytes, 4_096));

            var remaining = maximumBytes - (int)_capture.Length;

            if (remaining > 0)
                _capture.Write(bytes[..Math.Min(bytes.Length, remaining)]);

            if (bytes.Length > remaining)
                IsTruncated = true;
        }
        catch
        {
            _shouldCapture = false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        _disposed = true;

        if (disposing)
            _capture?.Dispose();

        base.Dispose(disposing);
    }
}