# RUSBP-Bootstrap

**RUSBP-Bootstrap** es una herramienta para la preparación y gestión segura de dispositivos USB en sistemas con autenticación multifactor y cifrado BitLocker, como parte del proyecto Recon USB Pass (RUSBP).

El objetivo es permitir limpiar, descifrar y preparar un **USB root** seguro para la administración central de credenciales y claves, integrándose con la infraestructura PKI y el backend centralizado.

---

## 🟩 **Flujo de uso (lógico y seguro)**

### INICIO

1. **Pedir la IP del Backend Sistema Central**
   - Solicita la IP (solo formato IPv4, sin http ni puerto)
   - Valida conectividad con ping
   - Si no responde, muestra advertencia y permite continuar bajo confirmación

2. **Detectar todas las unidades USB conectadas**
   - Si NO hay unidades → muestra error y solicita conectar una
   - Si hay unidades:
     - Muestra lista de unidades detectadas
     - Solicita seleccionar la unidad a trabajar
     - Advierte NO retirar la unidad durante el proceso

3. **Confirmar selección de unidad**
   - Muestra resumen de la unidad seleccionada (letra, etiqueta, tamaño, serial)

4. **Menú principal: Elegir acción a realizar**
   - **1. Cambiar IP del backend**
     - Permite ingresar una nueva IP y valida nuevamente conectividad
   - **2. Cambiar unidad USB**
     - Permite seleccionar otra unidad conectada
   - **3. Limpiar la unidad** (BORRAR TODO, NO descifra)
     - Confirma la acción
     - Si está cifrada, pide la clave BitLocker y desbloquea antes de limpiar
     - Borra todos los archivos posibles (ignora errores de sistema/protegidos)
   - **4. Descifrar la unidad** (quita BitLocker, no borra archivos)
     - Solicita clave BitLocker, intenta descifrar, muestra resultado
   - **5. Cifrar unidad con BitLocker** (si aún no está cifrada)
     - Advierte que la clave que ingrese será SOLO para el USB root (no agentes)
     - Solicita clave BitLocker root (mínimo 8 caracteres)
     - Cifra la unidad y muestra progreso
   - **6. Registrar root** (flujo normal)
     - Pide datos del usuario root-admin:
       - RUT, Nombre, Departamento, Email, PIN
     - Pide la clave "global" de los agentes empleados/administradores (NO root)
     - Advierte: "La clave BitLocker del root solo debe usarse en el USB root"
     - Pide la clave del root para leer/escribir los archivos
     - Genera claves PKI, guarda en el USB y obtiene serial y thumbprint
     - **Registra el USB** (serial, thumbprint), usuario y asociación en backend
     - **Guarda 3 archivos cifrados en `/rusbp.sys/`:**
         - `.btlk`         = clave root      (cifrada con pass_root)
         - `.btlk-agente`  = clave genérica  (cifrada con pass_root)
         - `.btlk-ip`      = IP backend      (cifrada con pass_root)
     - Crea `config.json` (sin datos sensibles)
     - Muestra advertencia: "NO uses este USB root en los agentes locales, salvo emergencia"
     - Muestra mensaje de éxito

7. **FIN**

---

## ⚡ Requisitos

- **Windows 10/11 Pro, Enterprise o Education** (para cifrado BitLocker)
- **.NET 8.0 o superior**
- **Permisos de administrador** (necesarios para BitLocker)

---

## 👩‍💻 Características Técnicas

- Detección robusta de USBs y selección asistida
- BitLocker opcional, con validación de edición de Windows y advertencias claras
- Lógica modularizada (helpers separados)
- Cifrado seguro de las claves dentro del propio USB root (AES, password root)
- Integración directa con backend (alta USB, usuario, asociación, PKI) vía API REST
- Generación de clave/certificado PKI para autenticación central
- Gestión de archivos `.btlk`, `.btlk-agente`, `.btlk-ip` para agentes y root
- Mensajes de flujo claros y protección ante errores de usuario o hardware

---

## 🛡️ Notas de seguridad y mejores prácticas

- **No retires el USB durante el proceso de cifrado ni configuración.**
- **Guarda las claves BitLocker (root y agentes) en entornos seguros, nunca las anotes en texto plano.**
- **El cifrado del USB root es altamente recomendado en ambientes productivos.**
- **NO uses el USB root para iniciar sesión en agentes, salvo emergencia. Si se usa, se deja registro y advertencia.**
- **Nunca dejes las claves dentro de archivos de configuración (`config.json`). Usa siempre los archivos `.btlk` cifrados.**

---

## 📝 Desarrollo y extensión

- Toda la lógica de seguridad y manejo de dispositivos está en la carpeta `Core/`
- Puedes agregar soporte para otras políticas de cifrado o extensiones siguiendo la arquitectura modular
- Para integración con nuevas APIs, basta extender `Api/BackendClient.cs` y sus modelos DTO

---

## 💡 Notas prácticas sobre el código

- En el flujo principal (`Program.cs`):
    - Pide la IP y valida antes de cualquier otra cosa
    - Pide y guarda las dos claves (root y agente) y la IP en archivos cifrados `.btlk`
- Usa siempre el helper de cifrado simétrico (AES, `CryptoHelper`) para leer/escribir los archivos `.btlk`
- El agente local, durante el setup, solo pide la clave root UNA VEZ para desencriptar y guardar todo localmente con DPAPI

---

## 🤝 Contacto y contribución

¿Tienes dudas, mejoras o sugerencias?  
Abre issues o pull requests en el repositorio principal del proyecto RUSBP.

---

**Hecho con ❤️ para la entrega y despliegue seguro de sistemas críticos.**
