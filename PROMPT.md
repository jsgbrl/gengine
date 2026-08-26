# gengine — prompt de construcción

> Pegá este archivo entero como primer mensaje de una sesión nueva en
> `C:\bin\projects\claude\gengine`.
>
> El prompt está en español; **todo lo que produzcas está en inglés**.
> Las decisiones de acá ya están tomadas: implementalas, no las vuelvas a discutir. Si
> encontrás una contradicción real, señalala en una línea y seguí con la lectura más razonable.

## 1. Qué tiene que existir cuando termines

`gengine`: un mini motor de juego 2D en C# sobre .NET 10 que dibuja en la **consola**, con
**física 2D propia**, **joystick DualSense (PS5) por USB** y un **clon de Mario Bros** jugable
de punta a punta. Es material didáctico: el repositorio se lee como un curso sobre cómo está
hecho un motor por dentro.

Actuás como ingeniero de motores **y como docente**. Cada archivo tiene que funcionar y tiene
que enseñar: alguien que sabe C# pero nunca escribió un motor debería poder leer el repo entero
y entender cada decisión.

Terminaste cuando estos comandos funcionan, sin ningún paso previo:

```bash
dotnet run run.cs
```
```bash
dotnet run tests.cs
```
```bash
dotnet build gengine.slnx -warnaserror
```
```bash
dotnet format gengine.slnx --verify-no-changes --severity error
```

Más los cuatro ejemplos, por ejemplo `dotnet run examples/02-bouncing-balls.cs`.

## 2. Las diez reglas

Ninguna es negociable.

1. **.NET 10** (`net10.0`), C# 14. El SDK instalado es `10.0.400`.
2. **Cero NuGet en todo el repositorio**, tests incluidos: nada de xUnit, MonoGame, Raylib,
   Terminal.Gui, Spectre.Console, HidSharp, StyleCop ni Sonar. Solo la BCL y los analizadores
   que ya trae el SDK. `NuGet.config` en la raíz con `<packageSources><clear /></packageSources>`,
   para que un `PackageReference` accidental **rompa el build** en vez de colarse.
3. **Se juega como un script**: `run.cs` es una *file-based app* que referencia los proyectos
   con `#:project`. Nunca hace falta un `dotnet build` a mano para jugar.
4. **Un solo binario portable**: cero `#if`, cero RIDs. Lo específico de plataforma se resuelve
   en runtime (`OperatingSystem.IsWindows()` y compañía) detrás de una interfaz.
5. **Windows, macOS y Linux en pie de igualdad.** Ninguno es el principal, ninguno el degradado:
   mismas funciones, mismos valores por defecto, mismo tacto de juego. Si un sistema no puede
   algo, se degrada **igual en los tres**, anunciado en pantalla, nunca con un crash ni en
   silencio.
6. **P/Invoke sí** —no es NuGet, y sin él no hay joystick—, siempre detrás de una interfaz.
   **`unsafe` no.** `dynamic` tampoco. Reflexión solo en el runner de tests y en la carga de
   recursos embebidos.
7. **Todo determinista**: ni `DateTime.Now`, ni `Environment.TickCount`, ni `Random` sin semilla
   en la lógica de juego o de física. El tiempo entra por `IClock`.
8. **Todo en inglés**: identificadores, nombres de archivo, comentarios, XML docs, README,
   `docs/`, HUD, menús, mensajes de error y salida de los tests. Ni una palabra en español en el
   artefacto.
9. **Cada comportamiento observable tiene un test que se pone rojo si lo rompés.** No hay cuota
   de cantidad; sí está prohibido el test que llama a un método y no afirma nada.
10. **No declares terminado lo que no ejecutaste.** Pegá la salida real. Si algo falla, mostralo;
    si no lo pudiste probar —macOS sin una Mac, el joystick sin el joystick—, decilo con todas
    las letras. Un error reportado vale más que una promesa sin ejecutar.

## 3. Cómo se tiene que leer el código

Requisito de primer orden, al mismo nivel que "compila". Al cerrar un archivo, el lector tiene
que quedarse con la sensación de que todo estaba en su lugar y de que era más simple de lo que
temía.

**Presupuestos.** Archivo ≤ 250 líneas, tipo ≤ 150, método ≤ 20, anidamiento ≤ 3, parámetros ≤ 4.
Si no entra, falta un tipo. Única excepción: las clases `NativeMethods`, que son firmas P/Invoke
sin lógica; quedan exentas del límite de líneas y así está declarado en el linter.

**Forma.** Guard clauses y salida temprana, sin `else` después de un `return`. Orden "diario" en
cada archivo: resumen, constantes, campos, constructor, propiedades, métodos públicos, y recién
ahí los privados, en el orden en que se los llama por primera vez. Simetría visible: todo `Add`
con su `Remove`, todo `Enter` con su `Exit`, juntos y en ese orden. Constantes en tablas
alineadas y con la unidad en el nombre (`JumpVelocityPixelsPerSecond`, no `jumpV`). Sin
`#region`, sin ternarios anidados, sin LINQ ingenioso en el camino caliente. `var` solo cuando
el tipo se lee a la derecha. Sin parámetros booleanos en APIs públicas: enums con nombre. Sin
abreviaturas fuera de las del dominio (`id`, `aabb`, `hid`, `rgb`, `fps`, `dt`).

**Vocabulario.** Un concepto, una palabra, en todo el repo: si es `body`, nunca es `object`,
`actor` ni `thing`. `docs/glossary.md` lo fija y el código no se desvía. Los nombres se leen en
voz alta como una frase: `if (player.IsGrounded && jumpBuffer.IsActive)`.

**Comentarios.** Cada archivo abre con 1–3 líneas sobre su papel en el conjunto. Ningún
comentario repite el código: explican el porqué, la invariante o la unidad, o citan la fuente
(*Fix Your Timestep*). Cada proyecto tiene su `README.md` de una pantalla.

**Pasada final.** Antes de entregar releés todos los archivos como lector, no como autor, y
arreglás lo que no fluye. Listá en el informe qué archivos tocó esa pasada. Un archivo que no
aguanta la lectura en voz alta no está terminado.

## 4. Quién hace cumplir todo esto: la máquina, no el criterio

Cuatro capas, todas con herramientas del SDK.

**(a) Analizadores al máximo**, en `src/Directory.Build.props` y `tests/Directory.Build.props`:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>disable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest-all</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Dos de esas líneas hacen el trabajo pesado. `EnforceCodeStyleInBuild` hace que las reglas
`IDExxxx` se evalúen **durante el build** y no solo dentro del IDE: sin eso, el `.editorconfig`
es decoración. `GenerateDocumentationFile` enciende `CS1591`, con lo cual **un miembro público
sin XML doc no compila**.

`latest-all` prende cientos de reglas y algunas van a pelearse con el estilo didáctico —`CA1303`
se queja de cada literal que pasa por `Console.Write`—. **No bajes las propiedades globales para
hacerlas callar**: se ajusta regla por regla en `.editorconfig`, cada una con un comentario que
la justifique, y el listado va a `docs/style.md`. Si estás apagando muchas, casi siempre el
problema es el código.

**(b) `.editorconfig` en la raíz, única fuente de verdad del estilo.** Formato base
(`charset = utf-8`, `end_of_line = lf`, 4 espacios, `insert_final_newline`,
`trim_trailing_whitespace`), `IDE0055` como error, las reglas que codifican §3 —llaves siempre,
`this.` prohibido, `var` acotado, `readonly` donde corresponda, `using` ordenados— y las reglas
de nombres (`dotnet_naming_rule`: interfaces con `I`, campos privados `_camelCase`, constantes
`PascalCase`, genéricos con `T`). Todas con severidad `error`, no `suggestion`. Un bloque aparte
para `tests/` puede aflojar lo que haga falta, también documentado.

**Verificá cada clave contra el build.** Si alguna no existe o no tiene efecto en .NET 10,
sacala y anotalo: no dejes claves inventadas dando una falsa sensación de control.

**(c) `dotnet format --verify-no-changes --severity error`**, que también viene en el SDK.

**(d) Un linter propio**, en `GEngine.Architecture.Tests`, para lo que ningún analizador puede
saber porque son reglas de este proyecto. Cada fallo reporta **archivo, línea y regla violada**:

| Test | Verifica |
|---|---|
| `StyleRulesTests` | Los presupuestos de §3; un tipo público por archivo; resumen al inicio; sin `#region`, `TODO` ni código muerto. |
| `VocabularyTests` | El código usa los términos de `docs/glossary.md`, no sus sinónimos. |
| `PlatformRulesTests` | Sin `#if`, sin `\` literal en rutas, sin API de un solo SO fuera de su backend. |
| `DependencyRulesTests` | El grafo de referencias de §5. |
| `PackagingRulesTests` | Cero `PackageReference`, cero `#:package`. |
| `SuppressionRulesTests` | Cero `#pragma warning disable` en el código: toda supresión vive en `.editorconfig`. |

## 5. Arquitectura

`Core` no depende de nadie: define las interfaces (`IRenderer`, `IInputBackend`, `IPhysicsWorld`,
`IClock`, `IAssetSource`, `ILogger`, `IPlatformProbe`). Los tres módulos de implementación
dependen solo de `Core` y **nunca entre sí**. `MarioClone` los conoce a todos y arma la
composición. `DependencyRulesTests` falla si alguien rompe una flecha; no es burocracia, es el
ejemplo vivo de inversión de dependencias que el README explica.

```
              ┌───────────────┐
              │  MarioClone   │
              └───────┬───────┘
         ┌────────────┼────────────┐
         ▼            ▼            ▼
     Physics      Rendering      Input
         └────────────┼────────────┘
                      ▼
                GEngine.Core
```

```
gengine/
├─ README.md · LICENSE (MIT) · .gitignore
├─ .editorconfig · .gitattributes · NuGet.config · gengine.slnx
├─ run.cs                       ← el juego
├─ tests.cs                     ← todos los tests
├─ examples/
│  ├─ 01-hello-loop.cs          game loop desnudo: FPS y tiempo
│  ├─ 02-bouncing-balls.cs      física sin juego: gravedad, rebotes, restitución
│  ├─ 03-tilemap-physics.cs     un cuadradito que camina y salta sobre un tilemap
│  └─ 04-gamepad-probe.cs       vuelca reportes HID crudos y el estado decodificado
├─ src/
│  ├─ Directory.Build.props     ← acá, no en la raíz: puede romper los scripts
│  ├─ GEngine.Core/ · GEngine.Physics/ · GEngine.Rendering/
│  ├─ GEngine.Input/            teclado + HID + DualSense + mapeo de acciones
│  ├─ GEngine.Testing/
│  └─ MarioClone/               assets/*.sprite · levels/1-1.txt
├─ tests/                       un proyecto por módulo, más GEngine.Architecture.Tests
│  └─ GEngine.Input.Tests/fixtures/dualsense/   reportes reales en hexadecimal
└─ docs/
   architecture · physics · rendering · input-and-gamepad · platforms
   patterns · style · glossary · tutorial · tests
```

Un tipo público por archivo; carpeta = namespace.

## 6. Los módulos

### `GEngine.Core`

Matemática (`Vector2` como `readonly struct` de `float`, `Aabb`, `MathG`) con la API que se
espera de cada uno, comparaciones con épsilon y `IEquatable`. Más:

- `GameLoop` — **timestep fijo con acumulador**: `Update(dt)` variable, `FixedUpdate(1/60)`
  fijo, `Render(alpha)` interpolado. Clamp del frame time contra la *spiral of death*. El pacing
  es un híbrido sleep + spin que tolera la granularidad distinta de `Thread.Sleep` en cada SO.
- `IClock` / `StopwatchClock` / `ManualClock` — el tiempo, inyectable. `ManualClock` es lo que
  hace deterministas los tests.
- `Entity`, `Component` (Template Method), `Transform2D` con jerarquía y local ↔ mundo.
- `Scene` — altas y bajas **diferidas** por cola, para no mutar la colección mientras se itera.
  Explicá el bug clásico en `<remarks>`: es de los que más se sufren escribiendo un motor.
- `EventBus`, `StateMachine<TState>`, `Pool<T>` (contá por qué importa la presión sobre el GC),
  `ServiceRegistry` (Service Locator, con su advertencia de cuándo **no** usarlo).
- `IPlatformProbe` / `PlatformFactory` — detecta el SO y elige implementaciones. Inyectable,
  para poder testear los tres caminos desde cualquier máquina.

### `GEngine.Physics`

- `PhysicsWorld : IPhysicsWorld` — `Gravity`, `Step(fixedDt)`, `Add`/`Remove`, `Raycast`,
  `OverlapBox`, `QueryPoint`. Orden de iteración estable por Id, que es de dónde sale el
  determinismo.
- `RigidBody2D` — `BodyType { Static, Kinematic, Dynamic }`, masa e inversa, `GravityScale`,
  `Drag`, `Restitution`, `Friction`, `MaxVelocity`, `IsGrounded`, `Layer`/`Mask`, `IsTrigger`.
- **Integración** Euler semi-implícito (`v += a·dt; x += v·dt`); explicá en `<remarks>` por qué
  no el explícito y por qué no Verlet acá.
- **Broad phase** con dos implementaciones tras `IBroadPhase`: `BruteForceBroadPhase` (O(n²),
  didáctica) y `SpatialHashGrid` (la que se usa). Un test exige que **devuelvan exactamente el
  mismo conjunto de pares**: así se demuestra que optimizar no cambió el resultado.
- **Narrow phase** AABB con vector de mínima traslación, y **swept AABB** por *slabs* contra
  estáticos para que nada atraviese una pared a alta velocidad.
- **Resolución** por separación de ejes, X primero y después Y —el truco clásico de los
  plataformas—, más corrección posicional e impulso con restitución y fricción.
- **Eventos** `OnCollision*` y `OnTrigger*` con registro de contactos entre frames: cada uno
  dispara **exactamente una vez**. Capas por bitmask.
- Casos que un motor de plataformas necesita: *one-way* (sólida solo si el cuerpo cae y venía de
  arriba), plataformas cinemáticas que **arrastran** al pasajero, y `TileCollisionSource`, que
  colisiona contra un tilemap **sin un cuerpo por tile**.

### `GEngine.Rendering`

- `FrameBuffer` de píxeles con `Clear`, `SetPixel`, `DrawRect`, `DrawSprite`, `DrawText`, `Blit`
  y **recorte correcto en los cuatro bordes**. `Color` RGBA, `Palette` estilo NES,
  `BitmapFont` 5×7 en código para el HUD.
- **Sprites en texto plano** (`assets/*.sprite`): grilla de caracteres más leyenda
  carácter→color, `.` transparente, editables con un notepad. `IAssetSource` con variante
  embebida, de sistema de archivos y en memoria (tests).
- `ConsoleRenderer : IRenderer` — **medio bloque `▀`**, con el color de frente como píxel
  superior y el de fondo como inferior: píxeles cuadrados y el doble de resolución vertical.
  Truecolor ANSI, **diff contra el frame anterior** y una sola escritura por frame sobre un
  stream con buffer grande. Nada de `Console.Clear()` por frame, que es de donde viene el
  parpadeo. Buffer alternativo, cursor oculto, detección de resize.
- `IConsoleDriver` con las tres implementaciones: Windows habilita VT con `SetConsoleMode`; en
  macOS y Linux ya viene, pero igual se verifica y se informa. Sin truecolor, degradá a 256 o 16
  colores **igual en los tres**, con aviso.
- **El terminal siempre queda usable**: `try/finally` + `CancelKeyPress` + `ProcessExit`, ante
  salida normal, `Esc`, `Ctrl+C` o excepción. Es requisito, no un extra.
- `Camera2D` con zona muerta y clamp al nivel. `HeadlessRenderer` + `AsciiSnapshot` para tests
  de captura.

### `GEngine.Input` — teclado y acciones

- `InputState` con `IsDown`, `WasPressedThisFrame`, `WasReleasedThisFrame`, `HoldTime`.
- `ConsoleKeyboardBackend`, **el mismo en los tres SO**, con decaimiento configurable.
  Documentá la limitación honesta: la consola no entrega eventos de key-up, y por eso el
  joystick es el camino preciso. Nada de un backend privilegiado para un sistema.
- `InputMap` + patrón **Command**: el juego consulta **acciones** (`MoveLeft`, `Jump`, `Run`,
  `Pause`), nunca teclas ni botones. Teclado y DualSense se mapean a las mismas acciones, así
  que **el código del juego no sabe de dónde viene la entrada**. Es la idea central de la capa.
- `InputRouter` fusiona las fuentes activas: se puede jugar solo con teclado, solo con joystick,
  o alternar **en plena partida sin pausar**.
- `FakeInputBackend` reproduce guiones de entrada, para tests y replays. Remapeo por archivo
  de texto.

### `GEngine.Input` — DualSense (PS5) por USB

Enchufás el joystick, corrés el juego y jugás. Sin drivers, sin NuGet, en los tres sistemas.

```
DualSenseGamepad   decodifica reportes → GamepadState → acciones del InputMap
      ▲
IHidDevice         abre / lee / cierra un dispositivo HID (64 bytes por reporte)
      ▲
IHidBackend        enumera por VID/PID
```

| SO | Mecanismo | Interop |
|---|---|---|
| Linux | Abrir `/dev/hidraw*` con `FileStream` y leer 64 bytes; enumerar por `HID_ID` en `/sys/class/hidraw/*/device/uevent`. | **Ninguno**: es I/O de archivos. |
| Windows | `SetupDiGetClassDevs` y familia para enumerar, `HidD_GetAttributes` para el VID/PID, `CreateFile` + `ReadFile` para leer. | `setupapi.dll`, `hid.dll`, `kernel32.dll`, las tres del sistema. |
| macOS | IOKit HID Manager (`IOHIDManagerCreate`, `SetDeviceMatching`, `Open`, `RegisterInputReportCallback`) con un `CFRunLoop` propio. | IOKit **y CoreFoundation**: el diccionario de filtrado se arma a mano y hay que manejar `CFRetain`/`CFRelease`. La parte más cara del proyecto. |

**Reglas del interop**, que importan porque chocan con otras reglas de este prompt:

- **`[DllImport]`, no `[LibraryImport]`**: el generador de `LibraryImport` emite stubs que exigen
  `AllowUnsafeBlocks`, prohibido por la regla 6.
- Por lo tanto **`SYSLIB1054` va suprimido, solo en las carpetas de interop**, en `.editorconfig`
  y con esa justificación textual. Con `latest-all` y warnings como errores, sin esa supresión el
  build no compila. Es la primera supresión legítima del proyecto y sirve de ejemplo para las
  demás. Revisá también `CA1060` y `CA5392`.
- `byte[]` y `Marshal`, nunca punteros. Handles en `SafeHandle`. Una clase `NativeMethods`
  `internal` por plataforma, **solo firmas**: la lógica vive en el backend, que es lo que se
  testea. El hilo de lectura va con `IsBackground = true`, porque un `ReadFile` bloqueado no
  puede impedir que el proceso termine.

**Comportamiento.** `GamepadState` normalizado: sticks en `[-1, 1]`, gatillos en `[0, 1]`, botones
con la misma semántica de flancos que el teclado. Zona muerta **radial** configurable —explicá en
`<remarks>` por qué se siente mejor que la de por eje—. La lectura nunca bloquea el game loop:
hilo dedicado y último estado publicado. **Hot-plug**: desenchufar en plena partida pausa con un
mensaje, reenchufar sigue. Por defecto, D-pad y stick izquierdo mueven, Cross salta, Square
corre, Options pausa.

**Solo USB.** Bluetooth usa otro report ID, otro corrimiento y CRC: queda en el roadmap. Si
detectás un DualSense por Bluetooth, avisá con un mensaje claro en vez de decodificar basura.

**El formato del reporte está en el apéndice, y es una hipótesis, no un dogma.** Verificalo con
`examples/04-gamepad-probe.cs` contra hardware real antes de escribir el decodificador.
**Si no tenés un DualSense a mano**: implementá contra el apéndice, dejá el probe listo y
usable, marcá los fixtures como no verificados con hardware, anotalo en
`docs/input-and-gamepad.md` y **decilo en el informe final**. No presentes como medido lo que
copiaste de una tabla.

**Requisitos por sistema**, en `docs/platforms.md` y en el README: en Linux `/dev/hidraw*` suele
ser solo para root y hace falta una regla de udev —es el primer paso, no una nota al pie—; en
macOS puede pedir permiso de *Input Monitoring* al terminal, y si se niega hay que degradar a
teclado explicando cómo concederlo; en Windows el dispositivo puede estar tomado por Steam o
DS4Windows, así que se abre con acceso compartido y, si falla, se dice con nombre y apellido.

**Testeable sin hardware**: `FakeHidDevice` reproduce reportes guardados como fixtures en
hexadecimal. Decodificación, zona muerta, flancos y hot-plug se testean sin joystick enchufado.

### `GEngine.Testing`

Como no hay NuGet, el framework de tests es parte del entregable, y es en sí mismo una lección
de cómo funcionan xUnit y compañía por dentro.

Atributos `[Test]`, `[TestCase(...)]`, `[Setup]`, `[Skip("reason")]`. Un `Assert` con lo
esperable más `ApproximatelyEqual` para floats y `MatchesSnapshot` para capturas; los mensajes
de fallo muestran esperado contra obtenido. `TestRunner` descubre por reflexión, cronometra,
imprime con color y devuelve exit code, con `--filter`, `--list` y `--verbose`.

### `MarioClone`

- **Nivel** en `levels/1-1.txt`, grilla de caracteres con leyenda (`#` suelo, `B` ladrillo,
  `?` sorpresa, `o` moneda, `P` tubería, `G` goomba, `F` meta, `M` spawn). Nivel nuevo = editar
  un `.txt`.
- **Mario**: aceleración y fricción, tope de caminata y de corrida, derrape al cambiar de
  dirección, **altura de salto variable** (soltar corta el ascenso), **coyote time** y **jump
  buffer** de ~6 frames, velocidad terminal, estados `Small`/`Big`, invulnerabilidad, muerte y
  respawn. Todas las constantes de tacto viven en un único `PlayerTuning`, para poder
  experimentar con ellas.
- Con joystick el stick izquierdo da **movimiento analógico**; con teclado, el mismo
  `PlayerTuning` da el equivalente digital. Tiene que sentirse igual de bien con los dos.
- **Goomba**: patrulla, gira al chocar o al borde de la plataforma, muere pisado (normal hacia
  arriba y Mario cayendo) y lo rebota; lo mata por contacto lateral.
- **Bloques**: el `?` sube con un tween y entrega una sola vez; el ladrillo se rompe si Mario es
  grande y si no rebota. **Recolectables**: monedas y hongo.
- **Cámara** con zona muerta que **no retrocede**, como el original. **HUD** con puntaje,
  monedas, mundo, tiempo y vidas.
- **Estados**: `Title → Playing → Paused → Death → LevelComplete → GameOver`. El título muestra
  los controles **de la fuente detectada**: botones si hay joystick, teclas si no.
- **Audio**: `IAudioBackend` con `NullAudioBackend` por defecto. Nada de `Console.Beep`, que
  solo existe en Windows y rompería la regla 5.

### Patrones

Usalos y **nombralos en los comentarios** donde aparecen: Game Loop, Update Method, Component,
State, Observer, Command, Object Pool, Service Locator, Strategy (broad phase), Template Method,
Factory (plataforma), Adapter (backends HID), Flyweight (sprites). `docs/patterns.md` lista cada
uno con archivo y línea.

## 7. Qué hay que testear

Además del linter de §4, y todo con `ManualClock` y dobles de prueba: **ningún test depende del
tiempo real ni de hardware**.

**Core** — casos límite del álgebra (vector cero, normalizar el cero, cajas que se tocan justo
en el borde); que 1.0 s dé exactamente 60 `FixedUpdate` y que el clamp corte la spiral of death;
altas y bajas **durante** la iteración de la escena; `StateMachine`, `Pool<T>`, `EventBus`; que
`PlatformFactory` elija bien en los tres SO simulados.

**Physics** — caída libre contra la solución analítica; penetración y MTV; **tunneling**: a
10 000 px/s no se atraviesa una pared de 1 px; un cuerpo en reposo no tiembla ni se hunde tras
10 000 pasos; restitución 0 y 1; triggers que disparan Enter y Exit una sola vez; one-way;
plataforma móvil que arrastra al pasajero; **brute force y spatial hash dan lo mismo**;
**determinismo**: misma entrada, mismo hash de estado tras 10 000 pasos; nunca NaN ni infinito.

**Rendering** — recorte en los cuatro bordes; transparencia; round-trip world↔screen; zona
muerta y clamp; que el diff **no emita bytes** si el frame no cambió; snapshots de una escena
conocida; degradación a 16 colores.

**Input** — flancos en frames consecutivos, buffer, remapeo, guion exacto del backend falso, y
que `InputRouter` permita cambiar de fuente en caliente sin perder un frame. Del DualSense:
cada botón y las 8 direcciones del hat desde fixtures; normalización de sticks (centro exacto →
`Vector2.Zero`); zona muerta radial; gatillos; flancos entre reportes; hot-plug simulando fallas
de lectura; enumeración que ignora otros dispositivos HID; Bluetooth detectado y rechazado.

**MarioClone** — altura de salto dentro de tolerancia; coyote time y jump buffer; pisar al
goomba lo mata y rebota a Mario; el goomba gira en el borde; la moneda suma y desaparece; el
pozo mata; el `?` entrega una sola vez. Y el **replay de integración**: un guion grabado lleva a
Mario del spawn a la meta sin renderizar, corrido una vez como teclado y otra como gamepad,
**y las dos tienen que llegar**.

**Rendimiento** — **cero asignaciones por frame** en régimen, medido con
`GC.GetAllocatedBytesForCurrentThread` sobre `HeadlessRenderer`.

**Cobertura de API** — todo tipo público del motor tiene su clase `*Tests`, con una lista
explícita y justificada de exenciones (enums, DTOs).

## 8. README

En inglés, con diagramas en Mermaid o ASCII y sin imágenes externas. Ocho bloques:

1. Qué es y qué no es, más una captura ASCII del juego.
2. **Arrancar en 10 segundos**: requisitos, `dotnet run run.cs`, controles de teclado y de
   DualSense, y la **matriz de soporte** SO × subsistema con los requisitos de cada sistema
   (ninguna celda dice "no soportado": dice qué hace falta).
3. Los cuatro ejemplos y qué enseña cada uno.
4. Arquitectura: el diagrama de capas y por qué `Core` no depende de nadie.
5. **Cómo funciona el game loop**: timestep variable, acumulador, interpolación.
6. **Cómo funciona la física**: integración, broad y narrow phase, swept AABB, resolución por
   ejes, one-way. Con fórmulas.
7. **Cómo se lee un joystick sin librerías**: qué es HID, cómo se enumera en cada SO, y un
   reporte real anotado byte por byte.
8. **Tu primer juego en 30 líneas** (código real que compila), cómo crear un nivel, cómo correr
   y agregar tests, mapa del código, roadmap, glosario y licencia.

## 9. Plan de trabajo

No avances de fase con tests en rojo o con warnings. Al terminar cada una informá qué quedó
hecho, la salida real de los tests y qué sigue.

| Fase | Qué | Entregable visible |
|---|---|---|
| 0 | Verificá `dotnet --list-sdks` y que un `hello.cs` corra con `dotnet run` **antes** de construir nada encima. Esqueleto, `NuGet.config`, `.editorconfig`, `.gitattributes`, `Directory.Build.props`, **con los analizadores ya al máximo**: encenderlos al final es reescribir todo. | El build vacío pasa |
| 1 | `GEngine.Testing` y sus propios tests, rojo → verde a mano | `dotnet run tests.cs` |
| 2 | `GEngine.Core` | `examples/01-hello-loop.cs` |
| 3 | `GEngine.Physics` | `examples/02` en texto |
| 4 | `GEngine.Rendering` | `examples/02` a color |
| 5 | `GEngine.Input`: teclado, acciones, router | `examples/03` jugable |
| 6 | `GEngine.Input`: HID y DualSense. **Primero el probe y la verificación del reporte**, después el decodificador | `examples/04` |
| 7 | `MarioClone`, con el replay de integración | `dotnet run run.cs` |
| 8 | README, `docs/`, pasada de legibilidad de §3, verificación final | La entrega |

Dos trampas del entorno que conviene tener presentes: `Directory.Build.props` va en `src/` y
`tests/` porque en la raíz puede romper los scripts file-based, y `run.cs` no puede quedar dentro
de la compilación de otro proyecto. Y si una API que pensabas usar no existe en .NET 10,
**verificá compilando**, no supongas.

## 10. Verificación final

Ejecutá los comandos de §1 y pegá la salida real. La entrega está completa cuando:

- [ ] Se completa el nivel 1-1 **solo con teclado** y **solo con el DualSense por USB**.
- [ ] Enchufar y desenchufar el joystick en plena partida no rompe nada.
- [ ] `dotnet run tests.cs` da exit code 0 sin hardware conectado y sin tests salteados sin
      justificar.
- [ ] `dotnet build` no emite **ni un** warning, con `AnalysisLevel=latest-all` puesto, y
      `dotnet format` no propone un solo cambio.
- [ ] Cero `#pragma warning disable`, cero `PackageReference`, cero `#if` de plataforma.
- [ ] Los cuatro ejemplos corren.
- [ ] Ni una palabra en español en el código, la documentación ni la pantalla.
- [ ] Ningún archivo ni método supera los presupuestos de §3, y la pasada de legibilidad está
      hecha y reportada.
- [ ] Al salir —normal, con `Esc`, con `Ctrl+C` o por excepción— la consola queda limpia.
- [ ] Está dicho explícitamente **qué verificaste con ejecución real y qué no**: en qué sistemas
      operativos, y con o sin joystick.

---

## Apéndice — reporte USB del DualSense

VID `0x054C`; PID `0x0CE6` (DualSense) y `0x0DF2` (Edge), en una tabla con nombre y fácil de
ampliar. Reporte `0x01`, 64 bytes. Ejes de 0 a 255, centro en 128, **Y invertido** (0 = arriba).

| Byte | Contenido |
|---|---|
| 0 | Report ID (`0x01`) |
| 1–2 | Stick izquierdo X, Y |
| 3–4 | Stick derecho X, Y |
| 5–6 | Gatillos analógicos L2, R2 |
| 7 | Contador de secuencia |
| 8 | Nibble bajo: D-pad como hat (0 = N, 1 = NE … 7 = NW, 8 = neutro). Nibble alto: `0x10` Square, `0x20` Cross, `0x40` Circle, `0x80` Triangle |
| 9 | `0x01` L1, `0x02` R1, `0x04` L2 digital, `0x08` R2 digital, `0x10` Create, `0x20` Options, `0x40` L3, `0x80` R3 |
| 10 | `0x01` PS, `0x02` clic del touchpad, `0x04` mute del micrófono |
| 11+ | Giroscopio, acelerómetro, touchpad, batería — fuera de alcance |

Esta tabla es una hipótesis de partida. **Medila con el probe antes de confiar en ella**; si un
bit no coincide, corregí las constantes y el fixture, y dejá anotado con qué modelo lo
verificaste.
