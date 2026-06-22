# SpatialRoute · XR Cargo Navigation

**Graph-based spatial guidance for PICO mixed-reality cargo retrieval.**

Unity runtime modules for cargo selection, aisle-aware route planning, deviation recovery, and in-view route guidance. Navigation topology, session logic, presentation, and storage are independent components.

## Architecture

```text
Cargo selection → cargo-to-node lookup → A* on the aisle graph
Tracked headset + map anchor → navigation session → route line / direction cue
                             ↘ off-route replanning / arrival detection
Anchor-local navigation map ↔ validated, versioned JSON storage
PICO video see-through → mixed-reality presentation
```

## Core capabilities

- **Navigation graph:** uniquely identified waypoints, cargo locations, directed or bidirectional aisle links, disabled links, and traversal-cost multipliers.
- **A* planning:** shortest graph path with a Euclidean lower-bound heuristic. Cost multipliers are at least one so the heuristic remains admissible.
- **Session state:** idle, navigating, paused, arrived, and unreachable, with state and route-change events.
- **Deviation recovery:** horizontal distance to remaining route segments triggers a rate-limited replan from the nearest graph node.
- **Anchor-relative coordinates:** maps store local positions, while guidance and arrival thresholds use world-space distances.
- **Persistence:** schema versioning, validation before applying imported maps, temporary-file replacement, and backup of the previous map.
- **PICO presentation:** video see-through, route-line rendering, and a configurable direction-arrow transform.

## Sources

- `Runtime/Navigation/NavigationGraph.cs` — graph/cargo schema and validation.
- `Runtime/Navigation/AStarPlanner.cs` — weighted aisle routing.
- `Runtime/Navigation/CargoNavigationSession.cs` — target selection, state transitions, route progression, and replanning.
- `Runtime/Presentation/RouteGuidancePresenter.cs` — world-space route rendering and horizontal direction cues.
- `Runtime/Persistence/RouteMapStore.cs` — versioned JSON import/export.
- `Runtime/Pico/PassthroughEnabler.cs` — PICO video-see-through integration.
- `Editor/NavigationGraphEditor.cs` — Inspector validation command.
- `Samples~/WarehouseMap.json` — illustrative two-aisle map in meters.

## Unity integration

1. Import the sources into a Unity 2022.3 project with the PICO Unity Integration SDK and an XR-tracked camera. The PICO adapter uses `Unity.XR.PXR`; the navigation modules do not depend on the PICO SDK.
2. Create an asset using **Create → SpatialRoute → Navigation graph**. Enter traversable aisle waypoints and edges, then associate each cargo identifier with a destination node. Use the Inspector validation button before navigation.
3. Add `CargoNavigationSession` to a scene object and assign the graph, map-origin transform, and tracked headset. `NavigateTo("A")` starts a session; `Pause`, `Resume`, `Cancel`, and `Retry` control it. An unreachable route clears the displayed path and emits its state.
4. Add `RouteGuidancePresenter` with a LineRenderer and assign the session and an optional arrow. The arrow's local +Z axis is its forward direction; its position remains available for your world-space or headset-relative layout.
5. Call `RouteMapStore.LoadInto(path, graph)` to load the sample or a saved layout. Store persistent maps beneath `Application.persistentDataPath`. Finish an active session before replacing its map; start a new session after import.
6. Align `mapOrigin` with the physical aisle coordinate frame using the application's spatial-anchor/alignment input. Keep the map floor plane horizontal. Entry-to-nearest-node access must be traversable; graph links describe allowed aisle movement.

Connect spatial-anchor observations through `mapOrigin`. Applications can update edge availability and request replanning as new aisle information arrives.

## Repository scope

Core C# modules and a small example map. Imported PICO/XR SDKs, Unity caches, application scenes, media, and APKs are excluded.

MIT license applies to these application sources; PICO and Unity dependencies retain their respective licenses.
