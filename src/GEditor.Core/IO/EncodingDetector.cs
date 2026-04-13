using System.Text;
using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>编码检测器 — BOM 优先 + 回退策略</summary>
public sealed class EncodingDetector : IEncodingDetector
{
    public DocumentEncodingInfo Detect(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        return Detect(bytes);
    }

    public DocumentEncodingInfo Detect(byte[] fileBytes)
    {
        if (fileBytes is null || fileBytes.Length == 0)
            return new DocumentEncodingInfo
            {
                Encoding = Encoding.UTF8,
                HasBom = false,
                DisplayName = "UTF-8"
            };

        // Check BOM
        if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
        {
            return new DocumentEncodingInfo
            {
                Encoding = Encoding.UTF8,
                HasBom = true,
                DisplayName = "UTF-8 with BOM"
            };
        }

        if (fileBytes.Length >= 2 && fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
        {
            return new DocumentEncodingInfo
            {
                Encoding = Encoding.Unicode, // UTF-16 LE
                HasBom = true,
                DisplayName = "UTF-16 LE"
            };
        }

        if (fileBytes.Length >= 2 && fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
        {
            return new DocumentEncodingInfo
            {
                Encoding = Encoding.BigEndianUnicode, // UTF-16 BE
                HasBom = true,
                DisplayName = "UTF-16 BE"
            };
        }

        // Heuristic: check for null bytes (likely UTF-16 without BOM)
        if (fileBytes.Length >= 4)
        {
            bool hasNullAtEven = false;
            bool hasNullAtOdd = false;
            int checkLen = Math.Min(fileBytes.Length, 4096);

            for (int i = 0; i < checkLen; i++)
            {
                if (fileBytes[i] == 0)
                {
                    if (i % 2 == 0) hasNullAtEven = true;
                    else hasNullAtOdd = true;
                }
            }

            if (hasNullAtEven && !hasNullAtOdd)
            {
                return new DocumentEncodingInfo
                {
                    Encoding = Encoding.BigEndianUnicode,
                    HasBom = false,
                    DisplayName = "UTF-16 BE"
                };
            }
            if (hasNullAtOdd && !hasNullAtEven)
            {
                return new DocumentEncodingInfo
                {
                    Encoding = Encoding.Unicode,
                    HasBom = false,
                    DisplayName = "UTF-16 LE"
                };
            }
        }

        // Default: UTF-8 without BOM
        return new DocumentEncodingInfo
        {
            Encoding = new UTF8Encoding(false),
            HasBom = false,
            DisplayName = "UTF-8"
        };
    }
}
