# Roadmap

Trabaja UNA etapa por sesión. Antes de escribir código, presenta un plan y espera aprobación. No empieces una etapa nueva sin que se te pida. La app debe compilar y ejecutarse al final de cada etapa.

## Etapa 1 — Solución y arquitectura onion

Misma estructura de cuatro proyectos: `CollabNotes.Domain` (entidades `Note`, `Folder`, `NotePermission`, enum de roles: Owner/Editor/Reader), `CollabNotes.Application` (servicios e interfaces: `INoteService`, `ICollaborationService`), `CollabNotes.Infrastructure` (EF Core) y `CollabNotes.Web` (MVC + SignalR Hub). El Hub de SignalR vive en Web pero delega toda la lógica a Application — el Hub debe ser tan delgado como un controlador.

## Etapa 2 — CRUD de notas sin tiempo real

Identity para autenticación, y el ciclo completo de notas: crear, editar (aún con un formulario normal que guarda al enviar), listar por carpetas, eliminar. Modela desde ya `NotePermission` como tabla intermedia (`NoteId`, `UserId`, `Role`) aunque todavía no compartas nada. Al final de esta etapa tienes una app de notas monousuario completamente funcional — eso ya es un fallback presentable si el tiempo real se complica.

## Etapa 3 — Compartir y permisos

Funcionalidad de invitar a otro usuario a una nota por email/username con rol de editor o lector. En el servicio, cada operación valida el permiso: un Reader no puede guardar cambios, solo el Owner puede eliminar o invitar. Esta lógica en Application es puro material de entrevista sobre autorización a nivel de negocio.

## Etapa 4 — Primer contacto con SignalR: presencia

Antes de sincronizar texto, haz lo fácil: un `NoteHub` con grupos por nota (`Groups.AddToGroupAsync(Context.ConnectionId, noteId)`), de modo que al abrir una nota veas quién más está conectado ("Ana está viendo esta nota") y cuando alguien entra o sale se actualice en vivo. Esto te enseña el modelo de grupos y conexiones de SignalR con bajo riesgo.

## Etapa 5 — Sincronización de contenido

El salto grande. Estrategia recomendada para portafolio: sincronización por bloques/párrafos con "última escritura gana". El cliente JavaScript escucha el evento `input`, aplica un debounce de ~300ms, y envía al Hub el bloque modificado; el Hub valida el permiso y lo retransmite al resto del grupo con `Clients.OthersInGroup(...)`. Guarda en la base de datos con el mismo debounce. Documenta honestamente en el README que no implementaste CRDT/OT y por qué — reconocer los límites de tu solución comunica más seniority que fingir que no existen.

## Etapa 6 — Experiencia colaborativa

Los detalles que hacen el demo memorable: cursores o indicador de "Ana está escribiendo…", resaltado del bloque que otro usuario edita, y manejo de reconexión (SignalR lo trae con `withAutomaticReconnect`, tú solo muestras el estado "reconectando…"). Historial simple de versiones si quieres un extra: guarda un snapshot cada N minutos.
