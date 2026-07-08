using OmniCore.Modules.Hash.Abstractions.Enums;
using OmniCore.Modules.Hash.Abstractions.Extensions;
using OmniCore.Modules.Hash.Abstractions.Interfaces;
using OmniCore.Modules.Hash.Abstractions.Models;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;

namespace OmniCore.Modules.Hash.Abstractions.Abstractions
{
    /// <summary>
    /// Базовый класс для провайдеров хеширования.
    /// Реализует общую логику работы с файлами, потоками и форматированием.
    /// </summary>
    public abstract class HashProvider : IHashProvider
    {
        /// <summary>
        /// Размер буфера по умолчанию для чтения потоков (80 KB)
        /// </summary>
        protected const int DefaultBufferSize = 81920;

        /// <summary>
        /// Сообщение об ошибке, когда поток не поддерживает чтение
        /// </summary>
        protected const string StreamCannotBeReadMessage = "The stream does not support reading.";

        /// <summary>
        /// Сообщение об ошибке, когда файл не найден
        /// </summary>
        protected const string FileNotFoundMessage = "File not found";

        /// <inheritdoc/>
        public abstract HashProviderMetadata Metadata { get; }

        #region Public Methods

        /// <inheritdoc/>
        public string Calculate(string input, Encoding? encoding = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            encoding ??= Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(input);
            byte[] hash = ComputeHash(bytes);
            return hash.ToHexString();
        }

        /// <inheritdoc/>
        public string Calculate(string input, HashOutputFormat format, Encoding? encoding = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            encoding ??= Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(input);
            byte[] hash = ComputeHash(bytes);
            return hash.ToFormattedString(format);
        }

        /// <inheritdoc/>
        public string Calculate(string filePath, HashOutputFormat format = HashOutputFormat.LowerHex)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(FileNotFoundMessage, filePath);
            }

            using FileStream stream = File.OpenRead(filePath);
            return Calculate(stream, format);
        }

        /// <inheritdoc/>
        public string Calculate(Stream inputStream, HashOutputFormat format = HashOutputFormat.LowerHex)
        {
            ArgumentNullException.ThrowIfNull(inputStream);

            if (!inputStream.CanRead)
            {
                throw new ArgumentException(StreamCannotBeReadMessage, nameof(inputStream));
            }

            byte[] hash = ComputeHash(inputStream);
            return hash.ToFormattedString(format);
        }

        /// <inheritdoc/>
        public async Task<string> CalculateAsync(
            string filePath,
            HashOutputFormat format = HashOutputFormat.LowerHex,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(FileNotFoundMessage, filePath);
            }

            await using FileStream stream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: DefaultBufferSize,
                useAsync: true);

            return await CalculateAsync(stream, format, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<string> CalculateAsync(
            Stream inputStream,
            HashOutputFormat format = HashOutputFormat.LowerHex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputStream);

            if (!inputStream.CanRead)
            {
                throw new ArgumentException(StreamCannotBeReadMessage, nameof(inputStream));
            }

            byte[] hash = await ComputeHashAsync(inputStream, cancellationToken).ConfigureAwait(false);
            return hash.ToFormattedString(format);
        }

        /// <inheritdoc/>
        public byte[] CalculateRaw(Stream inputStream)
        {
            ArgumentNullException.ThrowIfNull(inputStream);

            if (!inputStream.CanRead)
            {
                throw new ArgumentException(StreamCannotBeReadMessage, nameof(inputStream));
            }

            return ComputeHash(inputStream);
        }

        /// <inheritdoc/>
        public byte[] CalculateRaw(string filePath)
        {
            ArgumentNullException.ThrowIfNull(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден", filePath);
            }

            using FileStream stream = File.OpenRead(filePath);
            return CalculateRaw(stream);
        }

        /// <inheritdoc/>
        public async Task<byte[]> CalculateRawAsync(
            Stream inputStream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputStream);

            if (!inputStream.CanRead)
            {
                throw new ArgumentException(StreamCannotBeReadMessage, nameof(inputStream));
            }

            return await ComputeHashAsync(inputStream, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<byte[]> CalculateRawAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден", filePath);
            }

            await using FileStream stream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: DefaultBufferSize,
                useAsync: true);

            return await CalculateRawAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Вычислить хеш для массива байтов (синхронно)
        /// </summary>
        /// <param name="data">Входные данные</param>
        /// <returns>Массив байтов хеша</returns>
        protected abstract byte[] ComputeHash(byte[] data);

        /// <summary>
        /// Вычислить хеш для потока (синхронно)
        /// </summary>
        /// <param name="inputStream">Входной поток (должен поддерживать чтение)</param>
        /// <returns>Массив байтов хеша</returns>
        protected abstract byte[] ComputeHash(Stream inputStream);

        /// <summary>
        /// Вычислить хеш для потока (асинхронно)
        /// </summary>
        /// <param name="inputStream">Входной поток (должен поддерживать чтение)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Массив байтов хеша</returns>
        protected abstract Task<byte[]> ComputeHashAsync(Stream inputStream, CancellationToken cancellationToken);

        #endregion

        #region Protected Helper Methods

        /// <summary>
        /// Чанковое чтение потока для синхронного/асинхронного вычисления хеша через <see cref="HashAlgorithm"/>
        /// </summary>
        /// <param name="algorithm">Алгоритм хеширования</param>
        /// <param name="inputStream">Входной поток</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера (по умолчанию 80 KB)</param>
        /// <returns>Массив байтов, представляющий хеш</returns>
        protected static async Task<byte[]> ReadStreamWithTransformAsync(
            HashAlgorithm algorithm,
            Stream inputStream,
            CancellationToken cancellationToken,
            int bufferSize = DefaultBufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await inputStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            algorithm.TransformFinalBlock([], 0, 0);
            return algorithm.Hash!;
        }

        /// <summary>
        /// Чанковое чтение потока для асинхронного вычисления хеша через <see cref="NonCryptographicHashAlgorithm"/>
        /// </summary>
        /// <param name="algorithm">Алгоритм хеширования</param>
        /// <param name="inputStream">Входной поток</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="bufferSize">Размер буфера (по умолчанию 80 KB)</param>
        /// <returns>Массив байтов, представляющий хеш</returns>
        protected static async Task<byte[]> ReadStreamWithTransformAsync(
            NonCryptographicHashAlgorithm algorithm,
            Stream inputStream,
            CancellationToken cancellationToken,
            int bufferSize = DefaultBufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await inputStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                algorithm.Append(buffer.AsSpan(0, bytesRead));
            }

            return algorithm.GetCurrentHash();
        }

        #endregion
    }
}