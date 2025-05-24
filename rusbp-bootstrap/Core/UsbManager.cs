using rusbp_bootstrap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace rusbp_bootstrap.Core
{
    public class UsbDeviceInfo
    {
        public string DeviceId { get; set; } = "";
        public string? VolumeLabel { get; set; }
        public string? DriveLetter { get; set; } // Ej: "F"
        public ulong? SizeBytes { get; set; }
        public string? Serial { get; set; } // Serial físico USB, SIEMPRE EN MAYÚSCULAS Y HEX
        public bool IsMounted => !string.IsNullOrEmpty(DriveLetter);
    }

    public static class UsbManager
    {
        /// <summary>
        /// Devuelve TODAS las unidades USB físicas conectadas, incluso si no están montadas o están cifradas.
        /// El serial retornado es el hardware serial (extracto hexadecimal del PNPDeviceID), nunca el DeviceId ni la letra.
        /// </summary>
        public static List<UsbDeviceInfo> ListAllUsbDevices()
        {
            var result = new List<UsbDeviceInfo>();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementObject drive in searcher.Get())
            {
                var deviceId = drive["DeviceID"]?.ToString();
                ulong? size = drive["Size"] != null ? Convert.ToUInt64(drive["Size"]) : (ulong?)null;
                string? serial = null;

                // 1. Extraer serial desde PNPDeviceID (más confiable con BitLocker)
                var pnp = drive["PNPDeviceID"]?.ToString();
                if (!string.IsNullOrEmpty(pnp))
                {
                    // Extrae el bloque hexadecimal largo después del último '\'
                    // Ejemplo: USB\VID_0781&PID_5567\040174132B54FB6E1E0E...
                    var match = Regex.Match(pnp, @"\\([0-9A-Fa-f]{16,})");
                    if (match.Success)
                        serial = match.Groups[1].Value.Length >= 20
                            ? match.Groups[1].Value.Substring(0, 20).ToUpperInvariant()
                            : match.Groups[1].Value.ToUpperInvariant();
                }

                // 2. Si por alguna razón no hay serial aún, intenta SerialNumber directo
                if (string.IsNullOrWhiteSpace(serial))
                {
                    var serialRaw = drive["SerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serialRaw))
                        serial = serialRaw.ToUpperInvariant();
                }

                // 3. Si aún no hay serial, fallback: volumen serial (NO recomendado, sólo para dispositivos sin serial físico)
                if (string.IsNullOrWhiteSpace(serial) && deviceId != null)
                {
                    string? driveLetter = null;
                    // Intenta asociar una letra de unidad si existe
                    var partitionQuery = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceId}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                    foreach (ManagementObject partition in new ManagementObjectSearcher(partitionQuery).Get())
                    {
                        var logicalQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
                        foreach (ManagementObject logical in new ManagementObjectSearcher(logicalQuery).Get())
                        {
                            driveLetter = logical["DeviceID"]?.ToString().Replace(":", "");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(driveLetter) && GetVolumeInformation($@"{driveLetter}:\", null, 0, out uint volSer, out _, out _, null, 0))
                        serial = volSer.ToString("X8").ToUpperInvariant();
                }

                // Listar todas las particiones/volúmenes (montadas o no)
                var partitionQuery2 = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceId}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                bool foundAny = false;
                foreach (ManagementObject partition in new ManagementObjectSearcher(partitionQuery2).Get())
                {
                    var logicalQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
                    bool foundLetter = false;
                    foreach (ManagementObject logical in new ManagementObjectSearcher(logicalQuery).Get())
                    {
                        result.Add(new UsbDeviceInfo
                        {
                            DeviceId = deviceId!,
                            VolumeLabel = logical["VolumeName"]?.ToString(),
                            DriveLetter = logical["DeviceID"]?.ToString().Replace(":", ""),
                            SizeBytes = size,
                            Serial = serial // SIEMPRE en mayúsculas
                        });
                        foundLetter = true;
                        foundAny = true;
                    }
                    if (!foundLetter)
                    {
                        result.Add(new UsbDeviceInfo
                        {
                            DeviceId = deviceId!,
                            VolumeLabel = null,
                            DriveLetter = null,
                            SizeBytes = size,
                            Serial = serial
                        });
                        foundAny = true;
                    }
                }
                if (!foundAny)
                {
                    result.Add(new UsbDeviceInfo
                    {
                        DeviceId = deviceId!,
                        VolumeLabel = null,
                        DriveLetter = null,
                        SizeBytes = size,
                        Serial = serial
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Selecciona una unidad USB, desbloqueando con BitLocker si es necesario y posible.
        /// </summary>
        public static UsbDeviceInfo? SelectUsbDevice()
        {
            while (true)
            {
                var usbList = ListAllUsbDevices();
                if (usbList.Count == 0)
                {
                    Console.WriteLine("✖ No hay unidades USB conectadas. Inserte una para continuar...");
                    Console.ReadKey();
                    continue;
                }

                Console.WriteLine("Unidades USB detectadas:");
                for (int i = 0; i < usbList.Count; i++)
                {
                    var u = usbList[i];
                    var estado = u.IsMounted ? "" : "[CIFRADA o sin montar]";
                    var letra = u.DriveLetter != null ? (u.DriveLetter + ":") : "(sin letra)";
                    var size = u.SizeBytes != null ? (u.SizeBytes.Value / 1024 / 1024 / 1024) + " GB" : "Desconocido";
                    Console.WriteLine($"[{i + 1}] {letra} {estado} - Tamaño: {size}");
                }

                Console.Write("\nSeleccione el número de la unidad con la que desea trabajar: ");
                if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 1 || sel > usbList.Count)
                {
                    Console.WriteLine("Selección inválida.");
                    continue;
                }

                var selected = usbList[sel - 1];

                // Si NO está montada, intentar desbloquearla (BitLocker)
                if (!selected.IsMounted)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("La unidad seleccionada está cifrada o no montada.");
                    Console.WriteLine("¿Desea intentar desbloquearla con BitLocker? (S/N):");
                    Console.ResetColor();

                    var resp = Console.ReadLine()?.Trim().ToUpper();
                    if (resp == "S")
                    {
                        Console.WriteLine("Ingrese la clave BitLocker:");
                        string clave = Console.ReadLine()?.Trim() ?? "";
                        // UX estándar: el usuario lo desbloquea manualmente en Windows, luego repite.
                        Console.WriteLine("Intente desbloquearla manualmente desde Windows o PowerShell y vuelva a ejecutar el programa.");
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Operación cancelada.");
                        return null;
                    }
                }

                // Si tiene letra, devolver como válido
                return selected;
            }
        }

        // DLLImport para obtener serial por volumen (solo fallback, nunca como serial oficial)
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool GetVolumeInformation(
            string lpRootPathName,
            System.Text.StringBuilder? lpVolumeNameBuffer,
            int nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            System.Text.StringBuilder? lpFileSystemNameBuffer,
            int nFileSystemNameSize);
    }
}
