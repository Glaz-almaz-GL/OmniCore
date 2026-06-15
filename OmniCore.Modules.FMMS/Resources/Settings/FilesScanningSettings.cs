using OmniCore.Modules.FMMS.Enums;

namespace OmniCore.Modules.FMMS.Resources.Settings
{
    public sealed class FilesScanningSettings
    {
        public Dictionary<string, int> PagesCountCustomRules { get; set; } = [];

        public Dictionary<FileColumn, bool> ColumnVisibleSettings { get; set; } = new()
        {
            { FileColumn.Id, true },
            { FileColumn.Name, true },
            { FileColumn.PagesCount, true },
            { FileColumn.Extension, true },
            { FileColumn.SHA256, true },
            { FileColumn.SHA512, false },
            { FileColumn.MD5, false },
            { FileColumn.FullPath, false },
            { FileColumn.IsArchive, false },
            { FileColumn.IsArchiveEntry, false },
            { FileColumn.CompressedSize, false },
            { FileColumn.UnCompressedSize, false },
            { FileColumn.Size, true }
        };

        public HashSet<string> CustomArchiveExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".rar", ".7z", ".tar", ".gz", ".tgz", ".bz2", ".tb2", ".xz",
            ".txz", ".lz", ".tlz", ".z", ".lzma", ".lzo", ".ar", ".cpio", ".iso",
            ".dmg", ".wim", ".esd", ".squashfs", ".cramfs", ".jar", ".war", ".apk",
            ".xpi", ".epub", ".s7z"
        };

        public FileSizeType DisplayedSizeType { get; set; } = FileSizeType.MB;
        public bool ScanArchives { get; set; } = true;
        public bool CalculateSHA256 { get; set; } = true;
        public bool CalculateSHA512 { get; set; } = false;
        public bool CalculateMD5 { get; set; } = false;
    }
}