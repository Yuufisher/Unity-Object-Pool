# Unity Object Pool

[English](README.md) | 简体中文

一个轻量级的 Unity 对象池实现：用 `PoolManager` 统一管理多个对象池，用 `PoolableBehaviour` 作为池化对象的基类，避免频繁 `Instantiate` / `Destroy` 带来的 GC 与性能开销。

## 特性

- **按 key 分池**：同一类对象共用一个池，首次使用时自动创建
- **预热机制**：池创建时预先实例化一批对象，避免运行时卡顿
- **复用回调**：`Spawn()` / `Despawn()` 两个钩子，处理每次复用时的状态初始化与归零
- **空闲池上限**：归还时若空闲对象超过上限则直接销毁，控制内存占用
- **单例管理器**：`PoolManager` 在 `Awake` 中完成单例初始化并 `DontDestroyOnLoad`

## 目录结构

```
src/
├── PoolManager.cs     # PoolableBehaviour 基类 + PoolManager 单例管理入口
├── ObjectPool.cs      # 单个对象池的实现（Stack 空闲池 + HashSet 活跃池）
└── PooledCast.cs      # 示例：一个带 Rigidbody2D 的池化投射物
example/
├── ClickTOSpawn.cs    # 示例：鼠标左键点击时生成对象
├── PooledCast.cs      # 示例场景使用的副本
├── Scene/             # demo 场景与相关 Prefab
└── Prefab/            # Triangle.prefab
```

## 快速开始

### 1. 导入

把 `src/` 下的脚本复制到 Unity 工程的 `Assets` 目录（例如 `Assets/Plugins/ObjectPool/`）。

> 本仓库不是完整的 Unity 工程，没有 `ProjectSettings` 和 `Assets`，只是一个可直接拷入工程的源码包。

### 2. 搭建管理器

新建一个空物体，挂上 `PoolManager` 脚本。它会自动成为单例并跨场景保留，无需额外配置。

### 3. 编写池化对象

继承 `PoolableBehaviour`，实现两个抽象方法：

```csharp
using UnityEngine;

public class Bullet : PoolableBehaviour
{
    public float speed = 10f;

    public override void Spawn()
    {
        // 每次从池中取出时调用：重置位置、速度、颜色等
    }

    public override void Despawn()
    {
        // 归还时调用：停止协程、清空速度、复位 Transform
        StopAllCoroutines();
    }
}
```

**注意**：池化对象的预制体上必须挂有 `PoolableBehaviour` 的派生组件，否则 `ObjectPool` 在创建实例时会抛出异常。

### 4. 生成与归还

```csharp
// 生成：key 用于区分对象池，parent 是新实例的父节点
GameObject go = PoolManager.Instance.Spawn("bullet", bulletRoot, bulletPrefab);

// 归还：对象自己调用，内部通过 Owner 找到所属池
PoolManager.Instance.Despawn(GetComponent<PoolableBehaviour>());
```

完整示例见 [ClickTOSpawn.cs](example/ClickTOSpawn.cs) 与 [PooledCast.cs](example/PooledCast.cs)。

## API 参考

### PoolManager

| 成员 | 说明 |
| --- | --- |
| `PoolManager.Instance` | 全局静态实例，`Awake` 中赋值，重复实例会被销毁 |
| `Spawn(string key, Transform parent, GameObject prefab = null)` | 从池中取出对象；该 key 的池不存在时自动创建（预热 6 个） |
| `Despawn(PoolableBehaviour item)` | 归还对象，等价于 `item.Owner.Despawn(item)` |

### ObjectPool

构造函数：`ObjectPool(string key, Transform parent, GameObject pb = null, int prewarm = 10, int maxSize = 20)`

| 参数 | 说明 |
| --- | --- |
| `key` | 池的标识；当 `pb` 为 `null` 时用它从 `Resources/PoolPrefabs/{key}` 加载预制体 |
| `parent` | 所有实例的父节点 |
| `pb` | 预制体模板，为空则走 `Resources` 加载 |
| `prewarm` | 预热数量，默认 10 |
| `maxSize` | 空闲池上限，默认 20；归还时若超出则销毁对象 |

| 方法 | 说明 |
| --- | --- |
| `Spawn()` | 优先从空闲栈弹出，栈空则新建实例；返回 `GameObject` |
| `Despawn(PoolableBehaviour item)` | 从活跃集合移除，未超上限时失活并压回空闲栈，否则 `Destroy` |

### PoolableBehaviour

| 成员 | 说明 |
| --- | --- |
| `Owner` | 所属的 `ObjectPool`，由池在创建实例时注入 |
| `Spawn()` | 抽象方法，每次取出时调用 |
| `Despawn()` | 抽象方法，每次归还时调用 |

## 运行示例

1. 把 `src/` 与 `example/` 一起拷入工程的 `Assets`
2. 打开 `example/Scene/demo.unity`
3. 运行后点击鼠标左键，会从鼠标位置发射一个随机颜色、随机方向的三角形，2 秒后自动归还到池中

> 示例中的 `PooledCast` 使用了 `Rigidbody2D.linearVelocity`，需要 Unity 6（或 2023.3+）版本；更早的版本请改为 `velocity`。

## 已知限制

- `ObjectPool` 只在归还时限制空闲数量，活跃对象数量不受限
- 池一旦创建就不会自动销毁，`PoolManager` 中的字典也没有清理接口
- 非线程安全，只应在主线程调用
- 没有提供查询活跃对象、批量回收的接口

## License

MIT
