# Unity Object Pool

English | [简体中文](README.zh-CN.md)

A lightweight object pool for Unity. `PoolManager` owns and manages multiple pools keyed by name, while `PoolableBehaviour` serves as the base class for pooled objects — cutting down on the GC pressure and CPU cost of repeated `Instantiate` / `Destroy` calls.

## Features

- **Pool per key** — one pool per object type, created lazily on first use
- **Prewarming** — instances are created up front so gameplay doesn't hitch later
- **Reuse hooks** — `Spawn()` / `Despawn()` handle per-reuse initialization and state reset
- **Idle cap** — overflow objects are destroyed on return instead of accumulating in memory
- **Singleton manager** — `PoolManager` self-initializes in `Awake` and survives scene loads via `DontDestroyOnLoad`

## Project layout

```
src/
├── PoolManager.cs     # PoolableBehaviour base class + PoolManager singleton entry point
├── ObjectPool.cs      # Single pool implementation (Stack for idle, HashSet for active)
└── PooledCast.cs      # Sample: a pooled projectile driven by Rigidbody2D
example/
├── ClickTOSpawn.cs    # Sample: spawns objects on left mouse click
├── PooledCast.cs      # Copy used by the demo scene
├── Scene/             # Demo scene and related prefabs
└── Prefab/            # Triangle.prefab
```

## Getting started

### 1. Import

Copy the scripts under `src/` into your project's `Assets` folder (e.g. `Assets/Plugins/ObjectPool/`).

> This repository is not a complete Unity project — it has no `ProjectSettings` or `Assets` folder. It's a source drop you copy into an existing project.

### 2. Set up the manager

Create an empty GameObject and attach the `PoolManager` script. It becomes the singleton automatically and persists across scenes; no further configuration is needed.

### 3. Write a pooled object

Derive from `PoolableBehaviour` and implement the two abstract methods:

```csharp
using UnityEngine;

public class Bullet : PoolableBehaviour
{
    public float speed = 10f;

    public override void Spawn()
    {
        // Called each time the object is taken from the pool:
        // reset position, velocity, color, etc.
    }

    public override void Despawn()
    {
        // Called when returned: stop coroutines, clear velocity, reset the transform
        StopAllCoroutines();
    }
}
```

**Important:** the pooled prefab must have a `PoolableBehaviour`-derived component attached, otherwise `ObjectPool` throws when it creates an instance.

### 4. Spawn and despawn

```csharp
// Spawn: key identifies the pool, parent is the transform new instances are parented to
GameObject go = PoolManager.Instance.Spawn("bullet", bulletRoot, bulletPrefab);

// Despawn: called by the object itself; it resolves its owning pool through Owner
PoolManager.Instance.Despawn(GetComponent<PoolableBehaviour>());
```

See [ClickTOSpawn.cs](example/ClickTOSpawn.cs) and [PooledCast.cs](example/PooledCast.cs) for the full sample.

## API reference

### PoolManager

| Member | Description |
| --- | --- |
| `PoolManager.Instance` | Global static instance, assigned in `Awake`; duplicate instances are destroyed |
| `Spawn(string key, Transform parent, GameObject prefab = null)` | Takes an object from the pool, creating the pool (prewarmed to 6) on first use for that key |
| `Despawn(PoolableBehaviour item)` | Returns an object; equivalent to `item.Owner.Despawn(item)` |

### ObjectPool

Constructor: `ObjectPool(string key, Transform parent, GameObject pb = null, int prewarm = 10, int maxSize = 20)`

| Parameter | Description |
| --- | --- |
| `key` | Pool identifier; used to load the prefab from `Resources/PoolPrefabs/{key}` when `pb` is `null` |
| `parent` | Parent transform for all instances |
| `pb` | Prefab template; falls back to `Resources` loading when empty |
| `prewarm` | Number of instances created up front (default 10) |
| `maxSize` | Idle pool cap (default 20); objects beyond it are destroyed on return |

| Method | Description |
| --- | --- |
| `Spawn()` | Pops from the idle stack, instantiating a new object if empty; returns a `GameObject` |
| `Despawn(PoolableBehaviour item)` | Removes from the active set, then deactivates and pushes back onto the idle stack — or destroys it if the cap is exceeded |

### PoolableBehaviour

| Member | Description |
| --- | --- |
| `Owner` | The owning `ObjectPool`, injected by the pool when the instance is created |
| `Spawn()` | Abstract; called every time the object is taken from the pool |
| `Despawn()` | Abstract; called every time the object is returned |

## Running the demo

1. Copy both `src/` and `example/` into your project's `Assets`
2. Open `example/Scene/demo.unity`
3. Press Play and click the left mouse button — a triangle with a random color and direction launches from the cursor and returns to the pool after 2 seconds

> `PooledCast` uses `Rigidbody2D.linearVelocity`, which requires Unity 6 (or 2023.3+). On older versions, use `velocity` instead.

## Known limitations

- `ObjectPool` only caps the idle count; the number of active objects is unbounded
- Pools are never released once created, and `PoolManager` offers no way to clear its dictionary
- Not thread-safe — main thread only
- No API for querying active objects or bulk-recycling them

## License

MIT
