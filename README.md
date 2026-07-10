# CollabNotes

App de notas colaborativas en tiempo real, construida como proyecto de portafolio. ASP.NET Core MVC + SignalR, con arquitectura onion en cuatro proyectos.

## Stack

- .NET 9, ASP.NET Core MVC con Razor Views
- SignalR para colaboración en tiempo real
- Entity Framework Core + SQL Server (LocalDB en desarrollo)
- ASP.NET Core Identity para autenticación

## Arquitectura

- `CollabNotes.Domain` — entidades y enums, sin dependencias.
- `CollabNotes.Application` — interfaces de servicios/repositorios, DTOs, lógica de negocio y de autorización.
- `CollabNotes.Infrastructure` — EF Core, repositorios, Identity.
- `CollabNotes.Web` — controllers MVC, Razor Views, `NoteHub` (SignalR).

Detalle completo del roadmap en [`docs/ROADMAP.md`](docs/ROADMAP.md).

## Setup

```bash
dotnet restore
dotnet ef database update -p CollabNotes.Infrastructure -s CollabNotes.Web
dotnet run --project CollabNotes.Web
```

El cliente JS de SignalR (`wwwroot/lib/signalr/dist/browser/signalr.min.js`) ya está comiteado en el repo — no requiere `npm install` ni un paso de build adicional. Si por algún motivo falta, se puede descargar de nuevo desde `https://unpkg.com/@microsoft/signalr@8.0.7/dist/browser/signalr.min.js`.

La cadena de conexión por defecto apunta a `(localdb)\mssqllocaldb`. Ajustar `ConnectionStrings:DefaultConnection` en `appsettings.json`/`appsettings.Development.json` si se usa otra instancia de SQL Server.

## Sincronización de contenido: decisiones y límites

El contenido de una nota se sincroniza por **párrafo** ("bloque"), no por documento completo. El cliente debounce los cambios ~300ms y los envía al `NoteHub`, que valida el permiso del usuario y retransmite el bloque modificado al resto de los conectados a esa nota; el servidor persiste con el mismo debounce.

La estrategia de conflicto es **"last write wins" (última escritura gana)** por bloque — deliberadamente **no se implementó CRDT ni OT** (Conflict-free Replicated Data Types / Operational Transformation). Un CRDT u OT resolvería de forma determinística ediciones concurrentes en el mismo bloque, preservando ambos cambios cuando es posible, y evitando que la escritura de un usuario pise silenciosamente la de otro. Implementarlos correctamente es un problema de investigación por sí mismo (requiere estructuras de datos especializadas, manejo de causalidad, y normalmente un servidor de sincronización dedicado) — está fuera del alcance razonable de un proyecto de portafolio con tiempo acotado.

Limitaciones concretas y conocidas de esta implementación:

- **Identidad de bloque por índice.** Los "bloques" son párrafos separados por línea en blanco, identificados por su posición ordinal, no por un id estable. Si un usuario inserta o elimina un párrafo completo mientras otro está escribiendo en un párrafo más abajo, la edición en curso de ese segundo usuario puede terminar guardándose en el índice equivocado.
- **Última escritura gana, sin fusión.** Si dos personas editan el mismo bloque casi al mismo tiempo, gana quien el servidor procese último — la otra edición se pierde silenciosamente (no hay merge de texto).
- **Presencia y "escribiendo..." en memoria, sin backplane.** El estado de quién está viendo/escribiendo una nota vive en memoria del proceso (`ConcurrentDictionary`), no en una base de datos ni en un backplane tipo Redis. Funciona correctamente en una sola instancia; escalar a múltiples instancias del servidor requeriría agregar un backplane de SignalR.

El historial de versiones (snapshots periódicos, ver `Etapa 6` del roadmap) ayuda a mitigar el impacto de una escritura perdida, pero no la previene.
