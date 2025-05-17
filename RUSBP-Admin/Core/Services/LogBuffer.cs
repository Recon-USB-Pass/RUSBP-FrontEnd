using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Services
{
    /*
    /// <summary>Cifra en AES-256 un archivo logs.json.enc y envía batch al servidor.</summary>
    public class LogBuffer
    {
        private readonly string _filePath;
        private readonly byte[] _key;
        private readonly ApiClient _api;
        private readonly List<object> _buffer = new();
        private readonly object _lock = new();

        public LogBuffer(ApiClient api, string? filePath = null)
        {
            _api = api;
            _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "logs.json.enc");
            _key = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                           Environment.MachineName + "_rusbp"));

            LoadEncrypted();
        }

        public void Enqueue(object log)
        {
            lock (_lock)
            {
                _buffer.Add(log);
                SaveEncrypted();
            }
        }

        public async Task FlushAsync(CancellationToken ct = default)
        {
            List<object> copy;
            lock (_lock)
            {
                copy = _buffer.ToList();
                _buffer.Clear();
                SaveEncrypted();     // limpia
            }

            if (copy.Count == 0) return;

            var res = await _api.SendLogsAsync(copy, ct);
            if (!res.IsSuccessStatusCode) // si falla, re-agrega
            {
                lock (_lock)
                {
                    _buffer.AddRange(copy);
                    SaveEncrypted();
                }
            }
        }

        /* ───────────── Persistencia AES ───────────── *//*

        private void SaveEncrypted()
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            using var fs = File.Create(_filePath);
            fs.Write(aes.IV);

            using var cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
            JsonSerializer.Serialize(cs, _buffer);
        }

        private void LoadEncrypted()
        {
            if (!File.Exists(_filePath)) return;

            using var fs = File.OpenRead(_filePath);
            byte[] iv = new byte[16];
            if (fs.Read(iv) != 16) return;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read);
            var data = JsonSerializer.Deserialize<List<object>>(cs);
            if (data is not null) _buffer.AddRange(data);
        }
    }
    */
}
