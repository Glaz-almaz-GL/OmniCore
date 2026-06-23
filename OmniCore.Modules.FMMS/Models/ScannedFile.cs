using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.FMMS.Models
{
    public record class ScannedFile
    {
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long Size { get; set; }
        public int PagesCount { get; set; }
        public bool IsArchive { get; set; }
        public bool IsArchiveEntry { get; set; }
        public long CompressedSize { get; set; }
        public long UnCompressedSize { get; set; }

        public string? MD5 { get; set; }
        public string? SHA256 { get; set; }
        public string? SHA512 { get; set; }

        public string SizeFormatted { get; set; } = string.Empty;
    }
}
