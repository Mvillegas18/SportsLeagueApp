# Guía de sustentación en video — SportsLeagueApp

Este documento te sirve como **guion** para grabar el video de sustentación a nivel general (sin explicar línea por línea).

---

## 1) Objetivo del video (30–45 segundos)

Puedes iniciar diciendo algo como:

> "En este video voy a explicar la arquitectura general del proyecto SportsLeagueApp, los componentes que implementé para Sponsor y su relación con Tournament, y una demo funcional en Swagger con casos exitosos y de error."

---

## 2) Arquitectura N-Layer (1–2 minutos)

### Qué mostrar
- Estructura de solución con 3 proyectos:
  - `SportsLeague.API`
  - `SportsLeague.Domain`
  - `SportsLeague.DataAccess`

### Qué explicar
- `API`: capa de presentación (controllers, DTOs, mapeo, endpoints HTTP).
- `Domain`: reglas de negocio (entidades, enums, interfaces, servicios).
- `DataAccess`: persistencia con EF Core (`DbContext`, repositories, consultas).

### Mensaje clave
- Flujo general: **Controller → Service → Repository → DbContext/SQL Server**.

---

## 3) Capa Domain (2–3 minutos)

### Archivos a mostrar
- `SportsLeague.Domain/Entities/Sponsor.cs`
- `SportsLeague.Domain/Enums/SponsorCategory.cs`
- `SportsLeague.Domain/Entities/TournamentSponsor.cs`

### Qué explicar
1. `Sponsor`:
   - Propiedades principales: `Name`, `ContactEmail`, `Phone`, `WebsiteUrl`, `Category`.
   - Hereda de `AuditBase` (`Id`, `CreatedAt`, `UpdatedAt`).
   - Navigation: `ICollection<TournamentSponsor>`.

2. `SponsorCategory`:
   - `Main=0`, `Gold=1`, `Silver=2`, `Bronze=3`.
   - Explica que permite clasificar sponsors sin usar strings hardcodeados.

3. `TournamentSponsor` (entidad intermedia N:M):
   - FK: `TournamentId`, `SponsorId`.
   - Campos de negocio: `ContractAmount`, `JoinedAt`.
   - Navigation: `Tournament` y `Sponsor`.

### Mensaje clave
- La relación `Sponsor` ↔ `Tournament` es **muchos a muchos** mediante `TournamentSponsor`.

---

## 4) Capa DataAccess (2–3 minutos)

### Archivos a mostrar
- `SportsLeague.DataAccess/Context/LeagueDbContext.cs`
- `SportsLeague.Domain/Interfaces/Repositories/ISponsorRepository.cs`
- `SportsLeague.DataAccess/Repositories/SponsorRepository.cs`
- `SportsLeague.Domain/Interfaces/Repositories/ITournamentSponsorRepository.cs`
- `SportsLeague.DataAccess/Repositories/TournamentSponsorRepository.cs`

### Qué explicar
1. En `LeagueDbContext`:
   - `DbSet<Sponsor>` y `DbSet<TournamentSponsor>`.
   - Índice único en `Sponsor.Name`.
   - Relaciones `TournamentSponsor -> Tournament` y `TournamentSponsor -> Sponsor`.
   - Índice único compuesto `(TournamentId, SponsorId)` para evitar duplicados.

2. `SponsorRepository`:
   - Hereda de `GenericRepository<Sponsor>`.
   - Método `ExistsByNameAsync` para validar duplicados por nombre.

3. `TournamentSponsorRepository`:
   - `GetByTournamentAndSponsorAsync` para validar relación existente.
   - `GetBySponsorAsync` para listar torneos por sponsor.

### Mensaje clave
- La capa de acceso encapsula consultas EF Core y deja la lógica de negocio al service.

---

## 5) Capa API / Presentación (2–3 minutos)

### Archivos a mostrar
- DTOs:
  - `SportsLeague.API/DTOs/Request/SponsorRequestDTO.cs`
  - `SportsLeague.API/DTOs/Response/SponsorResponseDTO.cs`
  - `SportsLeague.API/DTOs/Request/TournamentSponsorRequestDTO.cs`
  - `SportsLeague.API/DTOs/Response/TournamentSponsorResponseDTO.cs`
- Mapeo: `SportsLeague.API/Mappings/MappingProfile.cs`
- Controller: `SportsLeague.API/Controllers/SponsorController.cs`
- Service:
  - `SportsLeague.Domain/Interfaces/Services/ISponsorService.cs`
  - `SportsLeague.Domain/Services/SponsorService.cs`

### Qué explicar
- DTOs separan contrato HTTP del modelo de dominio.
- AutoMapper reduce mapeo manual entre DTOs y entidades.
- `SponsorController` expone CRUD y endpoints de relación con torneos.
- Validaciones en `SponsorService`:
  - Nombre no duplicado.
  - Email válido.
  - Sponsor/Tournament deben existir.
  - No duplicar vínculo.
  - `ContractAmount > 0`.

### Mensaje clave
- El controller recibe la petición, el service aplica reglas, y el repository persiste/consulta.

---

## 6) Demo en Swagger (paso a paso) (3–5 minutos)

### Preparación
1. Ejecuta migraciones si aplica (`Update-Database`).
2. Levanta `SportsLeague.API`.
3. Abre Swagger (`/swagger`).

### Endpoint 1 — Crear sponsor
**POST** `/api/Sponsor`

Body ejemplo:
```json
{
  "name": "Nike",
  "contactEmail": "contacto@nike.com",
  "phone": "+52-5555555555",
  "websiteUrl": "https://nike.com",
  "category": 0
}
```

Qué decir:
- "Aquí se crea un sponsor y obtengo `201 Created` con el recurso creado."

---

### Endpoint 2 — Vincular sponsor a torneo
**POST** `/api/Sponsor/{id}/tournaments`

> Usa el `id` del sponsor recién creado y un torneo existente.

Body ejemplo:
```json
{
  "tournamentId": 1,
  "contractAmount": 1500000
}
```

Qué decir:
- "Si todo es válido, responde `201 Created` y se crea la relación en `TournamentSponsor`."

---

### Endpoint 3 — Listar torneos de un sponsor
**GET** `/api/Sponsor/{id}/tournaments`

Qué decir:
- "Este endpoint devuelve los torneos asociados al sponsor. Respuesta esperada: `200 OK`."

---

### Endpoint 4 — Actualizar sponsor
**PUT** `/api/Sponsor/{id}`

Body ejemplo:
```json
{
  "name": "Nike Updated",
  "contactEmail": "nuevocontacto@nike.com",
  "phone": "+52-5555550000",
  "websiteUrl": "https://www.nike.com/mx",
  "category": 1
}
```

Qué decir:
- "Si existe, responde `204 No Content` y actualiza campos del sponsor."

---

## Caso de error obligatorio (409)

Repite el vínculo del endpoint 2 con el mismo sponsor y torneo:

**POST** `/api/Sponsor/{id}/tournaments`
```json
{
  "tournamentId": 1,
  "contractAmount": 1500000
}
```

Qué decir:
- "Aquí demuestro la validación de no duplicar relación. El servicio detecta duplicado y responde `409 Conflict`."

---

## 7) Cierre sugerido (20–30 segundos)

Puedes cerrar con:

> "En resumen, implementé Sponsor y su relación N:M con Tournament respetando arquitectura N-Layer, validaciones de negocio en servicios, persistencia con repository pattern y exposición por API con DTOs y AutoMapper. También validé escenarios exitosos y de error en Swagger."

---

## Checklist rápido antes de grabar

- [ ] La API levanta y Swagger abre.
- [ ] Existe al menos un torneo en BD para vincular sponsor.
- [ ] Probé los 4 endpoints solicitados.
- [ ] Probé el error de duplicado para mostrar `409`.
- [ ] El video explica arquitectura y flujo, no línea por línea.
