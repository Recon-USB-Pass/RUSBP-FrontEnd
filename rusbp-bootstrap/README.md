# RUSBP-Bootstrap

**RUSBP-Bootstrap** es una herramienta para la preparación y gestión segura de dispositivos USB en sistemas con autenticación multifactor y cifrado BitLocker, como parte del proyecto Recon USB Pass (RUSBP).

El objetivo es permitir limpiar, descifrar y preparar un **USB root** seguro para la administración central de credenciales y claves, integrándose con la infraestructura PKI y el backend centralizado.

---

## 🟩 **Flujo de uso (lógico y seguro)**

INICIO

Detectar todas las unidades USB conectadas
└─ Si NO hay unidades → mostrar error y solicitar unidad. esperar a que se ingrese una unidad
└─ Si hay unidades:
→ Mostrar lista de unidades detectadas
→ Solicitar al usuario seleccionar la unidad a trabajar
→ Si se retira la unidad, avisar que no se retire en el proceso hasta que se termine, y terminar con un aprete una tecla para cerrar cmd

Confirmar selección de unidad
→ Mostrar resumen de la unidad seleccionada (letra, etiqueta, tamaño)

Menú principal: Elegir acción a realizar
├─ Opción 1: "Limpiar la unidad y descifrarla"
│ ├─ Advertir al usuario que la unidad será borrada y descifrada
│ ├─ Preguntar confirmación (S/N)
│ └─ Si NO confirma, volver a preguntar opciones del menú principal
│ └─ Si confirma:
│ ├─ Solicitar clave BitLocker (si está cifrada)
│ ├─ Intentar desbloquear y descifrar con la clave
│ ├─ Si éxito → continuar; Si error → mostrar mensaje y terminar
│ ├─ Borrar todos los archivos y carpetas de la unidad
│ └─ Mostrar mensaje "Unidad lista para reutilizar" y terminar
│
└─ Opción 2: "Crear/preparar USB root seguro (flujo normal)"
├─ Pedir datos del usuario root-admin:
│ ├─ RUT
│ ├─ Nombre
│ ├─ Departamento
│ ├─ Email
│ └─ PIN
│
├─ Detectar edición de Windows:
│ └─ Si NO es compatible con BitLocker:
│ ├─ Advertir que no se recomienda seguir sin cifrado
│ ├─ Preguntar si desea continuar sin cifrado (S/N)
│ └─ Si NO acepta [Entonces continuar pero con un signo de Warning al comienzo de todo lo que sigue]
│
├─ Si es compatible con BitLocker:
│ ├─ Recomendar cifrado
│ └─ Solicitar clave BitLocker (mínimo 8 caracteres)
│ └─ Si clave no cumple, pedir nuevamente con detalle para cumplir
│
├─ Obtener Serial del USB seleccionado
│ └─ Si no se obtiene, mostrar error y terminar
│
├─ (Si se usa BitLocker)
│ ├─ Cifrar la unidad seleccionada con la clave BitLocker
│ ├─ Mostrar progreso en consola (polling manage-bde)
│ └─ Guardar la clave BitLocker cifrada en el propio USB
│
├─ Registrar el USB en el backend vía API
├─ Registrar usuario root-admin en el backend vía API
├─ Asociar usuario ⇄ USB en el backend vía API
├─ Generar claves PKI y guardarlas en el USB
├─ Crear archivo config.json en el USB
└─ Mostrar mensaje de éxito "USB root preparado"

FIN

---

## 🗂️ **Estructura del Proyecto**

rusbp-bootstrap/
│
├── Program.cs // Flujo principal, menú y orquestador
├── Core/
│ ├── UsbManager.cs // Selección y serial de USBs
│ ├── BitLockerManager.cs // BitLocker (cifrado, descifrado, progreso)
│ ├── CryptoHelper.cs // AES para clave BitLocker
│ ├── PkiService.cs // Generación PKI
│ └── BootstrapHelpers.cs // Utilidades, prompts y edición de Windows
├── Api/
│ ├── BackendClient.cs // Cliente HTTP para la API central
│ └── Dtos.cs // Modelos para requests/responses API
├── Models/
│ ├── Usuario.cs // Modelo usuario root-admin
│ ├── UsbInfo.cs // Info completa unidad USB
│ └── ConfigJson.cs // Modelo para config.json
├── Properties/
│ └── launchSettings.json // Opcional para desarrollo/IDE
└── README.md

---

## ⚡ **Requisitos**

- **Windows 10/11 Pro, Enterprise o Education** (para cifrado BitLocker)
- **.NET 8.0 o superior**
- **Permisos de administrador** (requerido para gestionar BitLocker)

---

## 👩‍💻 **Características Técnicas**

- **Detección robusta de USBs** y selección asistida.
- **BitLocker opcional**, con validación de edición de Windows y mensajes de advertencia claros.
- **Lógica modularizada** (cada helper aislado en su archivo).
- **Cifrado seguro de la clave BitLocker** dentro del propio USB root (AES con password).
- **Integración directa con backend** (alta USB, usuario, asociación) vía API REST.
- **Generación de clave y certificado PKI** para la autenticación central.
- **Mensajes de flujo claros** y protección contra errores de usuario o hardware.

---

## 🛡️ **Notas de seguridad y mejores prácticas**

- **No retires el USB durante el proceso de cifrado ni configuración**.
- **Guarda la clave BitLocker en un entorno seguro y nunca fuera de los agentes autorizados.**
- **El cifrado del USB root es altamente recomendado para ambientes productivos.**

---

## 📝 **Desarrollo y extensión**

- Toda la lógica de seguridad y manejo de dispositivos está en la carpeta `Core/`.
- Puedes agregar soporte para otras políticas de cifrado o extensiones futuras siguiendo la arquitectura modular.
- Para integración con nuevas APIs, basta extender `Api/BackendClient.cs` y sus modelos DTO.

---

## 🤝 **Contacto y contribución**

Si tienes dudas, mejoras o sugerencias, puedes abrir issues o PR en el repositorio principal del proyecto RUSBP.

---

**Hecho con ❤️ para la entrega y despliegue seguro de sistemas críticos.**