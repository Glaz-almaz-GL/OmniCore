using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using System.Reflection;

namespace OmniCore.Hybrid.Interfaces
{
    /// <summary>
    /// Загрузчик модулей, отвечающий за обнаружение, загрузку сборок и создание экземпляров модулей.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Интерфейс абстрагирует процесс загрузки модулей из различных источников (файловая система,
    /// встроенные ресурсы, сеть) и предоставляет унифицированный API для получения экземпляров модулей.
    /// </para>
    /// <para>
    /// Загрузчик не отвечает за регистрацию сервисов модулей в DI-контейнере — это делает вызывающий код
    /// после получения экземпляров модулей.
    /// </para>
    /// </remarks>
    public interface IModuleLoader
    {
        /// <summary>
        /// Загружает все доступные модули из указанного источника.
        /// </summary>
        /// <param name="source">Источник загрузки (например, путь к директории с модулями).</param>
        /// <param name="currentOS">Текущая операционная система для фильтрации неподдерживаемых модулей.</param>
        /// <param name="serviceProvider">
        /// Провайдер сервисов для создания экземпляров модулей через DI.
        /// Может быть временным провайдером, созданным до финальной сборки контейнера.
        /// </param>
        /// <returns>Коллекция загруженных и инициализированных экземпляров модулей.</returns>
        /// <remarks>
        /// <para>
        /// Метод выполняет следующие шаги:
        /// <list type="number">
        /// <item><description>Загружает сборки из указанного источника</description></item>
        /// <item><description>Находит типы, реализующие <see cref="IModule"/></description></item>
        /// <item><description>Создает экземпляры модулей через DI-контейнер</description></item>
        /// <item><description>Фильтрует модули по поддержке ОС</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Модули, которые не прошли фильтрацию или вызвали ошибки при создании, логируются и пропускаются.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="source"/> или <paramref name="serviceProvider"/> равен <c>null</c>.
        /// </exception>
        IReadOnlyCollection<IModule> LoadModules(
            string source,
            OSPlatforms currentOS,
            IServiceProvider serviceProvider);

        /// <summary>
        /// Загружает сборки из указанного источника (директории).
        /// </summary>
        /// <param name="directoryPath">Путь к директории, содержащей DLL-файлы модулей.</param>
        /// <returns>Коллекция загруженных сборок.</returns>
        /// <remarks>
        /// <para>
        /// Метод загружает все <c>.dll</c> файлы из указанной директории.
        /// Нативные библиотеки и библиотеки несовместимой архитектуры игнорируются.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="directoryPath"/> равен <c>null</c>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Выбрасывается, если директория <paramref name="directoryPath"/> не найдена.
        /// </exception>
        IReadOnlyCollection<Assembly> LoadAssemblies(string directoryPath);

        /// <summary>
        /// Находит все типы модулей в загруженных сборках.
        /// </summary>
        /// <param name="assemblies">Коллекция сборок для сканирования.</param>
        /// <returns>Коллекция типов, реализующих <see cref="IModule"/> и подходящих для инстанцирования.</returns>
        /// <remarks>
        /// <para>
        /// Метод фильтрует типы по следующим критериям:
        /// <list type="bullet">
        /// <item><description>Тип реализует интерфейс <see cref="IModule"/></description></item>
        /// <item><description>Тип не является интерфейсом</description></item>
        /// <item><description>Тип не является абстрактным классом</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="assemblies"/> равен <c>null</c>.
        /// </exception>
        IReadOnlyCollection<Type> DiscoverModuleTypes(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Создает экземпляр модуля указанного типа.
        /// </summary>
        /// <param name="moduleType">Тип модуля для инстанцирования.</param>
        /// <param name="serviceProvider">Провайдер сервисов для внедрения зависимостей.</param>
        /// <param name="module">Созданный экземпляр модуля, если операция успешна; иначе <c>null</c>.</param>
        /// <returns><c>true</c>, если экземпляр успешно создан; иначе <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// Метод использует <c>ActivatorUtilities.CreateInstance</c> для создания экземпляра,
        /// что позволяет внедрять зависимости через конструктор.
        /// </para>
        /// <para>
        /// После создания экземпляра вызывается метод <see cref="IModule.Initialize"/>,
        /// передавая ему <paramref name="serviceProvider"/> для двухфазной инициализации.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="moduleType"/> или <paramref name="serviceProvider"/> равен <c>null</c>.
        /// </exception>
        bool TryCreateModuleInstance(Type moduleType, IServiceProvider serviceProvider, out IModule? module);

        /// <summary>
        /// Проверяет, поддерживается ли модуль текущей операционной системой.
        /// </summary>
        /// <param name="module">Модуль для проверки.</param>
        /// <param name="os">Операционная система.</param>
        /// <returns><c>true</c>, если модуль поддерживается; иначе <c>false</c>.</returns>
        /// <remarks>
        /// Метод использует побитовую операцию <c>&amp;</c> для проверки, предполагая, что
        /// <see cref="OSPlatforms"/> является флаговым перечислением.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="module"/> равен <c>null</c>.
        /// </exception>
        bool IsModuleSupported(IModule module, OSPlatforms os);
    }
}