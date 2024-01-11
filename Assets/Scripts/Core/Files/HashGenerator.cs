using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class HashGenerator {
    public static string GetFileHash(string filePath) {
        using (var sha256 = SHA256.Create()) {
            using (var stream = File.OpenRead(filePath)) {
                var hashBytes = sha256.ComputeHash(stream);
                return ConvertByteArrayToHexString(hashBytes);
            }
        }
    }

    public static string GetStringHash(string text) {
        using (var sha256 = SHA256.Create()) {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            var hashBytes = sha256.ComputeHash(textBytes);
            return ConvertByteArrayToHexString(hashBytes);
        }
    }

    private static string ConvertByteArrayToHexString(byte[] bytes) {
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; i++) {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}
