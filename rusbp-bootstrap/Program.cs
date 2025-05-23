// Program.cs – rusbp-bootstrap  (v3 – datos por consola + ayudas)
// -----------------------------------------------------------------------------
// 1) Detecta el primer pendrive disponible
// 2) Pregunta los datos del “Root-Admin” (con sugerencias)
// 3) Da de alta el USB (POST /api/usb)
// 4) Crea el usuario (POST /api/usuarios)
// 5) Asigna USB ⇄ Usuario   (POST /api/usb/asignar)
// 6) Genera PKI   (cert.crt / priv.key)
// 7) Graba config.json
//
// Compilar :  dotnet build -c Release
// Ejecutar  :  rusbp-bootstrap.exe
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

// ─────────────  CONFIG  ─────────────
const string apiBase = "https://192.168.1.209:8443";   // ← URL/IP del backend
// ────────────────────────────────────

Debug.WriteLine($"▲ bootstrap iniciado  –  API base: {apiBase}");

// --------------------------------------------------------------------
// 0) Datos del usuario root-admin
// --------------------------------------------------------------------
string Ask(string label, string hint, string def)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write($"{label} ");
    Console.ResetColor();
    Console.Write($"[{hint}]  (Enter = \"{def}\"): ");
    string? v = Console.ReadLine();
    return string.IsNullOrWhiteSpace(v) ? def : v.Trim();
}

string rut = Ask("RUT         :", "idealmente Jefe de TI", "11.111.111-1");
string nombre = Ask("Nombre      :", "nombre de la empresa", "admin");
string depto = Ask("Departamento:", "root / TI", "TI");
string email = Ask("Email       :", "correo corporativo", "admin@empresa.cl");
string pin = Ask("PIN         :", "un número inolvidable", "1234");
// Rol no se pregunta – siempre “Admin”

Console.WriteLine("\nInserta el pendrive destino y pulsa <Enter> …");
Console.ReadLine();

// --------------------------------------------------------------------
// 1) Pendrive y serial
// --------------------------------------------------------------------
var drive = DriveInfo.GetDrives()
                     .FirstOrDefault(d => d.DriveType == DriveType.Removable && d.IsReady);

if (drive is null)
{
    Console.WriteLine("✖  No se encontró ningún pendrive.");
    return;
}

Debug.WriteLine($"► Seleccionado: {drive.Name}  {drive.TotalSize / 1_073_741_824} GB");

string serial = UsbHelper.GetUsbSerial(drive.Name.TrimEnd('\\'));
Debug.WriteLine($"Serial obtenido: {serial}");
if (serial is "UNKNOWN" or "")
{
    Console.WriteLine("✖  Serial USB desconocido; abortando.");
    return;
}

// --------------------------------------------------------------------
// 2) HttpClient (omitir TLS sólo para la IP del backend)
// --------------------------------------------------------------------
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (req, cert, chain, errs) =>
        req.RequestUri!.Host == new Uri(apiBase).Host
};
using var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

// --------------------------------------------------------------------
// 3) Crear / registrar el USB
// --------------------------------------------------------------------
var usbDto = new { serial, thumbprint = "" };
Debug.WriteLine("POST /api/usb  →  " + JsonSerializer.Serialize(usbDto));
var usbResp = await http.PostAsJsonAsync("/api/usb", usbDto);
Debug.WriteLine($"Respuesta HTTP: {(int)usbResp.StatusCode} {usbResp.StatusCode}");
usbResp.EnsureSuccessStatusCode();

// --------------------------------------------------------------------
// 4) Crear usuario
// --------------------------------------------------------------------
var userDto = new
{
    rut,
    nombre,
    depto,
    email,
    rol = "Admin",
    pin
};
Debug.WriteLine("POST /api/usuarios  →  " + JsonSerializer.Serialize(userDto));
var usrResp = await http.PostAsJsonAsync("/api/usuarios", userDto);
Debug.WriteLine($"Respuesta HTTP: {(int)usrResp.StatusCode} {usrResp.StatusCode}");
usrResp.EnsureSuccessStatusCode();
var usuario = await usrResp.Content.ReadFromJsonAsync<UsuarioCreated>();
Debug.WriteLine($"Usuario creado.  id={usuario!.id}  msg={usuario.msg}");

// --------------------------------------------------------------------
// 5) Vincular USB ⇄ Usuario
// --------------------------------------------------------------------
var vincDto = new { serial, usuarioRut = rut };
Debug.WriteLine("POST /api/usb/asignar  →  " + JsonSerializer.Serialize(vincDto));
var vincResp = await http.PostAsJsonAsync("/api/usb/asignar", vincDto);
Debug.WriteLine($"Respuesta HTTP: {(int)vincResp.StatusCode} {vincResp.StatusCode}");
vincResp.EnsureSuccessStatusCode();

// --------------------------------------------------------------------
// 6) Generar PKI en el pendrive
// --------------------------------------------------------------------
var pkiDir = Path.Combine(drive.RootDirectory.FullName, "pki");
Directory.CreateDirectory(pkiDir);
Debug.WriteLine($"Generando PKI en {pkiDir} …");
var (certPath, keyPath) = PkiService.GeneratePkcs8KeyPair(serial, pkiDir);
Debug.WriteLine($"Cert   → {certPath}");
Debug.WriteLine($"Clave  → {keyPath}");

// --------------------------------------------------------------------
// 7) config.json
// --------------------------------------------------------------------
var cfg = new
{
    nombre,
    rut,
    email,
    rol = "Admin",
    Serial = serial,
    Fecha = DateTime.UtcNow
};
string cfgPath = Path.Combine(drive.RootDirectory.FullName, "config.json");
File.WriteAllText(cfgPath,
    JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
Debug.WriteLine($"config.json escrito en {cfgPath}");

// --------------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nUSB-ADM preparado ✔️  ¡Arranca la aplicación!");
Console.ResetColor();
Debug.WriteLine("▲ bootstrap finalizado correctamente");

// ═════════════════════════════════════════════════════════════════════
//  Modelos & helpers
// ═════════════════════════════════════════════════════════════════════
record UsuarioCreated(int id, string? msg);

// ---------- UsbHelper ------------------------------------------------
static class UsbHelper
{
    public static string GetUsbSerial(string driveLetter /* «F» ó «F:\» */)
    {
        driveLetter = driveLetter.TrimEnd('\\', ':');

        try
        {
            using var q = new ManagementObjectSearcher(
                $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}:'}} " +
                "WHERE AssocClass=Win32_LogicalDiskToPartition");

            foreach (ManagementObject part in q.Get())
            {
                using var disks = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} " +
                    "WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                foreach (ManagementObject disk in disks.Get())
                {
                    var sn = (disk["SerialNumber"]?.ToString() ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(sn))
                    {
                        Debug.WriteLine($"  WMI  → {sn}");
                        return sn.ToUpperInvariant();
                    }
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine("WMI error: " + ex.Message); }

        // Volcado de volumen si WMI falla
        if (GetVolumeInformation($@"{driveLetter}:\",
                null, 0, out uint volSer, out _, out _, null, 0))
            return volSer.ToString("X8");

        return "UNKNOWN";
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetVolumeInformation(
        string lpRootPathName,
        StringBuilder? lpVolumeNameBuffer,
        int nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        StringBuilder? lpFileSystemNameBuffer,
        int nFileSystemNameSize);
}

// ---------- PkiService -----------------------------------------------
static class PkiService
{
    public static (string certPath, string keyPath) GeneratePkcs8KeyPair(
        string commonName, string destDir)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            new X500DistinguishedName($"CN={commonName}"),
            rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow,
                                              DateTimeOffset.UtcNow.AddYears(5));

        string certPath = Path.Combine(destDir, "cert.crt");
        string keyPath = Path.Combine(destDir, "priv.key");

        File.WriteAllText(certPath,
            PemEncoding.Write("CERTIFICATE", cert.RawData));

        File.WriteAllText(keyPath,
            PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));

        return (certPath, keyPath);
    }
}
