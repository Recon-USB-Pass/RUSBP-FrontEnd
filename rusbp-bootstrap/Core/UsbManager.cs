using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using rusbp_bootstrap.Models;

namespace rusbp_bootstrap.Core
{
    public class UsbDeviceInfo
    {
        public string DeviceId { get; set; } = "";
        public string? VolumeLabel { get; set; }
        public string? DriveLetter { get; set; } // Ej: "F"
        public ulong? SizeBytes { get; set; }
        public bool IsMounted => !string.IsNullOrEmpty(DriveLetter);
    }

    public static class UsbManager
    {
        /// <summary>
        /// Devuelve TODAS las unidades USB físicas conectadas, incluso si no están montadas o cifradas.
        /// </summary>
        public static List<UsbDeviceInfo> ListAllUsbDevices()
        {
            var result = new List<UsbDeviceInfo>();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementObject drive in searcher.Get())
            {
                var deviceId = drive["DeviceID"].ToString();
                ulong? size = drive["Size"] != null ? Convert.ToUInt64(drive["Size"]) : (ulong?)null;
                string? serial = drive["SerialNumber"]?.ToString();

                // Buscar particiones (pueden no tener letra si está cifrada)
                var partitionQuery = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceId}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                var partitionSearcher = new ManagementObjectSearcher(partitionQuery);
                bool foundAny = false;

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    // Buscar letras asignadas (puede no haber ninguna)
                    var logicalQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
                    var logicalSearcher = new ManagementObjectSearcher(logicalQuery);
                    bool foundLetter = false;
                    foreach (ManagementObject logical in logicalSearcher.Get())
                    {
                        result.Add(new UsbDeviceInfo
                        {
                            DeviceId = deviceId!,
                            VolumeLabel = logical["VolumeName"]?.ToString(),
                            DriveLetter = logical["DeviceID"]?.ToString().Replace(":", ""),
                            SizeBytes = size
                        });
                        foundLetter = true;
                        foundAny = true;
                    }
                    // Si la partición no tiene letra (volumen no montado, bloqueado), igual la mostramos
                    if (!foundLetter)
                    {
                        result.Add(new UsbDeviceInfo
                        {
                            DeviceId = deviceId!,
                            VolumeLabel = null,
                            DriveLetter = null,
                            SizeBytes = size
                        });
                        foundAny = true;
                    }
                }
                // Si no hay partición, mostrar el disco físico igual
                if (!foundAny)
                {
                    result.Add(new UsbDeviceInfo
                    {
                        DeviceId = deviceId!,
                        VolumeLabel = null,
                        DriveLetter = null,
                        SizeBytes = size
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
                        // Intenta buscar la letra con powershell/get-bitlockervolume, aquí solo instruimos
                        // (Puedes automatizarlo, pero la UX estándar es que el usuario lo desbloquee y reintente)
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
    }
}
