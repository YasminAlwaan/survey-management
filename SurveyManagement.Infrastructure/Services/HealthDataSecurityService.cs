using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SurveyManagement.Core.Interfaces;
using SurveyManagement.Core.Models;

namespace SurveyManagement.Infrastructure.Services
{
    public class HealthDataSecurityService : IHealthDataSecurityService
    {
        private readonly ILogger<HealthDataSecurityService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly Dictionary<string, string> _tokenMap;

        public HealthDataSecurityService(
            ILogger<HealthDataSecurityService> logger,
            string encryptionKey)
        {
            _logger = logger;
            _encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);
            _tokenMap = new Dictionary<string, string>();
        }

        public string EncryptSensitiveData(string data)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.GenerateIV();

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var dataBytes = Encoding.UTF8.GetBytes(data);

                using var msEncrypt = new System.IO.MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                csEncrypt.Write(dataBytes, 0, dataBytes.Length);
                csEncrypt.FlushFinalBlock();

                var encryptedData = Convert.ToBase64String(msEncrypt.ToArray());
                var iv = Convert.ToBase64String(aes.IV);

                return $"{iv}:{encryptedData}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting sensitive data");
                throw new HealthDataSecurityException("Failed to encrypt sensitive data", ex);
            }
        }

        public string DecryptSensitiveData(string encryptedData)
        {
            try
            {
                var parts = encryptedData.Split(':');
                if (parts.Length != 2)
                    throw new HealthDataSecurityException("Invalid encrypted data format");

                var iv = Convert.FromBase64String(parts[0]);
                var cipherText = Convert.FromBase64String(parts[1]);

                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new System.IO.MemoryStream(cipherText);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new System.IO.StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting sensitive data");
                throw new HealthDataSecurityException("Failed to decrypt sensitive data", ex);
            }
        }

        public string TokenizePHI(string phiData)
        {
            if (string.IsNullOrEmpty(phiData))
                return null;

            var token = Guid.NewGuid().ToString("N");
            _tokenMap[token] = phiData;
            return token;
        }

        public string DetokenizePHI(string token)
        {
            if (string.IsNullOrEmpty(token) || !_tokenMap.TryGetValue(token, out string phiData))
                return null;

            return phiData;
        }

        public string MaskSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            // Mask email addresses
            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
                m => new string('*', m.Length));

            // Mask phone numbers
            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                @"\d{3}[-.]?\d{3}[-.]?\d{4}",
                m => new string('*', m.Length));

            // Mask SSN
            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                @"\d{3}[-.]?\d{2}[-.]?\d{4}",
                m => new string('*', m.Length));

            return data;
        }

        public void AuditDataAccess(string userId, string action, string entityType, string entityId)
        {
            _logger.LogInformation(
                "Data access audit: User={UserId}, Action={Action}, EntityType={EntityType}, EntityId={EntityId}",
                userId, action, entityType, entityId);
        }

        public bool ValidateDataRetentionPolicy(DateTime creationDate)
        {
            var retentionPeriod = TimeSpan.FromDays(365 * 7); // 7 years
            return DateTime.UtcNow - creationDate <= retentionPeriod;
        }
    }

    public class HealthDataSecurityException : Exception
    {
        public HealthDataSecurityException(string message) : base(message)
        {
        }

        public HealthDataSecurityException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
} 