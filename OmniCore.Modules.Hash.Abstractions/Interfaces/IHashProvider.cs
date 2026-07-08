using OmniCore.Modules.Hash.Abstractions.Enums;
using OmniCore.Modules.Hash.Abstractions.Models;
using System.Text;

namespace OmniCore.Modules.Hash.Abstractions.Interfaces
{
    /// <summary>
    /// Контракт для провайдеров хеширования. Каждый провайдер реализует конкретный алгоритм хеширования (например, MD5, SHA-256 и т.д.).
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Метаданные провайдера, включая имя алгоритма, размер хеша и категорию
        /// </summary>
        HashProviderMetadata Metadata { get; }

        #region Synchronous Methods

        /// <summary>
        /// Вычисление хеша для строки
        /// </summary>
        string Calculate(string input, Encoding? encoding = null);

        /// <summary>
        /// Вычисление хеша для строки с указанием формата вывода
        /// </summary>
        string Calculate(string input, HashOutputFormat format, Encoding? encoding = null);

        /// <summary>
        /// Вычисление хеша для файла
        /// </summary>
        string Calculate(string filePath, HashOutputFormat format = HashOutputFormat.LowerHex);

        /// <summary>
        /// Вычисление хеша для потока
        /// </summary>
        string Calculate(Stream inputStream, HashOutputFormat format = HashOutputFormat.LowerHex);

        /// <summary>
        /// Получение "сырого" хеша в виде байтового массива
        /// </summary>
        byte[] CalculateRaw(Stream inputStream);

        /// <summary>
        /// Получение "сырого" хеша в виде байтового массива для файла
        /// </summary>
        byte[] CalculateRaw(string filePath);

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// Асинхронное вычисление хеша для файла
        /// </summary>
        Task<string> CalculateAsync(string filePath, HashOutputFormat format = HashOutputFormat.LowerHex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Асинхронное вычисление хеша для потока
        /// </summary>
        Task<string> CalculateAsync(Stream inputStream, HashOutputFormat format = HashOutputFormat.LowerHex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Асинхронное получение "сырого" хеша в виде байтового массива
        /// </summary>
        Task<byte[]> CalculateRawAsync(Stream inputStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Асинхронное получение "сырого" хеша в виде байтового массива для файла
        /// </summary>
        Task<byte[]> CalculateRawAsync(string filePath, CancellationToken cancellationToken = default);

        #endregion
    }
}