using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers;
using System.Security.Cryptography;

static class Program
{
    static ILogger Logger = NullLogger.Instance;
    static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOpenApi();
        string[] allowedOrigins = builder.Configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>() ?? [];
        builder.Services.AddCors(options => options
            .AddPolicy("CorsPolicy", policy => policy
                .WithOrigins(allowedOrigins)
                .WithMethods(HttpMethods.Head, HttpMethods.Get, HttpMethods.Delete, HttpMethods.Put)
                .WithHeaders("Content-Type")));
        WebApplication app = builder.Build();
        Logger = app.Logger;
        app.UseCors("CorsPolicy");
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();
        app.MapMethods("/objects/{hash}", [HttpMethods.Head, HttpMethods.Get, HttpMethods.Delete], static (
            HttpRequest request,
            string hash,
            IWebHostEnvironment env) =>
        {
            if (GetPathFromHash(hash, stackalloc byte[SHA256.HashSizeInBytes]) is not string path)
                return Results.BadRequest("hash_format");
            path = Path.Join(env.ContentRootPath, "objects", path);
            if (!File.Exists(path))
                return Results.NotFound();
            if (HttpMethods.IsDelete(request.Method))
            {
                File.Delete(path);
                return Results.NoContent();
            }
            return Results.File(path, enableRangeProcessing: true);
        });
        app.MapPut("/objects/{hash}", static async (
            HttpRequest request,
            IWebHostEnvironment env,
            string hash,
            CancellationToken cancellationToken = default) =>
        {
            const int BufferSize = 65536;
            if (request.ContentLength is null)
                return Results.StatusCode(StatusCodes.Status411LengthRequired);
            byte[] sha256Hash = new byte[SHA256.HashSizeInBytes];
            if (GetPathFromHash(hash, sha256Hash) is not string path)
                return Results.BadRequest("hash_format");
            path = Path.Join(env.ContentRootPath, "objects", path);
            if (File.Exists(path))
                return Results.NoContent();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string tempPath = path + ".tmp";
            if (File.Exists(tempPath))
                return Results.Conflict();
            FileStream output;
            try
            {
                output = new(
                    tempPath,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.Read,
                    BufferSize,
                    useAsync: true);
            }
            catch (IOException ex)
            {
                Logger.LogError(ex, "Failed to create file {Path}", tempPath);
                return Results.Conflict();
            }
            try
            {
                await using (output)
                {
                    using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                    try
                    {
                        await using Stream input = request.Body;
                        while (true)
                        {
                            int read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                            if (read == 0)
                                break;
                            ReadOnlyMemory<byte> data = new(buffer, 0, read);
                            sha256.AppendData(data.Span);
                            await output.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                        }
                        Span<byte> receivedDataHash = buffer.AsSpan(0, sha256.GetCurrentHash(buffer));
                        if (receivedDataHash.SequenceEqual(sha256Hash))
                            goto OK;
                        return Results.BadRequest("hash_mismatch");
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            OK:
                File.Move(tempPath, path);
                return Results.Created();
            }
            finally
            {
                File.Delete(tempPath);
            }
        }).WithMetadata(new DisableRequestSizeLimitAttribute());
        app.Run();
    }
    static string? GetPathFromHash(string hash, Span<byte> hashBytes)
    {
        if (Convert.FromHexString(hash, hashBytes, out _, out int w) is not OperationStatus.Done
            || w != hashBytes.Length)
            return null;
        Span<char> buffer = stackalloc char[hashBytes.Length * 2 + 6];
        buffer[0] = ToHex(hashBytes[0] >> 4);
        buffer[1] = ToHex(hashBytes[0] & 0xF);
        buffer[2] = Path.DirectorySeparatorChar;
        buffer[3] = ToHex(hashBytes[1] >> 4);
        buffer[4] = ToHex(hashBytes[1] & 0xF);
        buffer[5] = Path.DirectorySeparatorChar;
        Convert.TryToHexStringLower(hashBytes, buffer[6..], out _);
        return new(buffer);
    }
    static char ToHex(int value)
        => (char)(value < 10 ? '0' + value : 'a' + value - 10);
}