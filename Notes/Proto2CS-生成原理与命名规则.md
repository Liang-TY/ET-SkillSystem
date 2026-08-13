# Proto2CS 生成原理与命名规则

> 分析 ET 项目中 `.proto` 文件如何生成 `.cs` 文件、opcode 机制、ET10 vs ET9 的差异。

---

## 一、Proto 文件命名规则

### 格式

```
{Name}_{C|S}_{OpcodeStart}.proto
```

| 部分 | 含义 | 示例 |
|------|------|------|
| `Name` | 协议名称/前缀，决定生成的 C# 类名 | `UBridge` → 生成 `public static class UBridge` |
| `C` 或 `S` | `C` = Client，`S` = Server | 决定生成到哪些目录（见第五节） |
| `OpcodeStart` | 5 位数字，opcode 起始值 | `10000` → 第一条消息 opcode=10001 |

### 示例

```
UBridge_C_10000.proto      → class UBridge, opcode 10001+
LockStepInner_S_21001.proto → class LockStepInner, opcode 21002+
RouterProto_C_1100.proto    → class RouterProto, opcode 1101+
```

### 数字范围

每文件可容纳任意数量消息，opcode 从 `startOpcode + 1` 开始自增。两个 proto 文件的 opcode 范围**不能重叠**，否则运行时 DoubleMap 报错。

---

## 二、Proto2CS 生成流程

### 入口

`Packages/cn.etetet.proto/DotNet~/Proto2CS.cs`

### 步骤

```
1. 扫描 packages-lock.json → 找到所有包的 Proto/ 目录
2. 遍历每个 .proto 文件
3. 解析文件名: "UBridge_C_10000" → protoName="UBridge", cs="C", startOpcode=10000
4. 逐行解析 proto 内容:
   - "// ResponseType Xxx" → 提取 ResponseType → 生成 [ResponseType(nameof(Xxx))]
   - "message XxxRequest // IRequest" → 生成 class + [Message(UBridge.XxxRequest)]
   - 字段 → 生成属性 + [MemoryPackOrder]
5. 生成静态 opcode 类:
   - public static class UBridge { public const ushort XxxRequest = 10001; ... }
6. 根据 cs 类型写入对应目录（见第五节）
```

### 生成产物

每个 `_C_` 前缀的 proto 文件生成 3 份 `.cs`（内容完全相同）：

```
CodeMode/Model/Client/UBridge_C_10000.cs      ← 无 asmref，编译到 Assembly-CSharp
CodeMode/Model/Server/UBridge_C_10000.cs      ← 无 asmref，编译到 Assembly-CSharp
CodeMode/Model/ClientServer/UBridge_C_10000.cs ← asmref → ET.Model
```

---

## 三、Opcode 机制

### 生成规则

```csharp
// Proto2CS.cs line 70-72
string protoName = ss[0];  // "UBridge"
string cs = ss[1];          // "C"
int startOpcode = int.Parse(ss[2]); // 10000

// line 138: 每条消息分配递增 opcode
msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

// line 141: 写入 Message 特性
[Message(UBridge.ConsoleGetLogsRequest)]

// line 230: 写入静态常量
public const ushort ConsoleGetLogsRequest = 10001;
```

### 运行时使用

```csharp
// OpcodeType.cs — ET 框架启动时扫描所有程序集
HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(MessageAttribute));
foreach (Type type in types)
{
    ushort opcode = messageAttribute.Opcode; // 从 [Message(UBridge.Xxx)] 读取
    if (opcode != 0)
    {
        this.typeOpcode.Add(type, opcode); // DoubleMap: Type ↔ opcode 双向映射
    }
    // 如果有 [ResponseType]，查找并注册 request ↔ response 映射
}
```

### 关键行为

- **opcode == 0**：OpcodeType 跳过，不加入路由表（UBridge 消息理论上应该是 0，因为不走网络）
- **opcode != 0**：加入 `typeOpcode`（DoubleMap）
- **Duplicate opcode**：`DoubleMap.Add` 检查 key 和 value 是否重复 → `vk.ContainsKey(value)` 检测到相同 opcode→ 抛异常

---

## 四、为什么拆分会报错

### 错误现象

```
double map add fail: ET.InspectorGetComponentsRequest 21002
```

### 根因链

```
Proto2CS: UBridgeEdit_C_21000.proto → 生成 3 份 .cs
    ↓
Client/UBridgeEdit_C_21000.cs          → Unity 编译 → Assembly-CSharp
Server/UBridgeEdit_C_21000.cs          → Unity 编译 → Assembly-CSharp  
ClientServer/UBridgeEdit_C_21000.cs   → asmref → ET.Model (dotnet 编译)
    ↓
运行时 CodeTypes 扫描 Assembly-CSharp + ET.Model 两个程序集
    ↓
同一个 InspectorGetComponentsRequest 在两个程序集中 opcode 都是 21002
    ↓
DoubleMap.vk.ContainsKey(21002) == true → 抛异常
```

### 为什么原来不报错（单文件）

原来单文件 `UBridge_C_10000.proto` 也生成 3 份（Client/Server/ClientServer），也存在同样的问题。但错误被**前置错误掩盖**了：

```
not found responseType: ET.RectGetRequest
```

`OpcodeType.Awake()` 中 ResponseType 解析先抛 Exception → 初始化中断 → typeOpcode 的双重插入还没走到。

RouterProto 等其他 proto 文件也有 3 份拷贝，为什么不报 opcode 冲突？因为它们的 opcode 值恰好和 UBridge 不重叠，且各自的 Client/Server 拷贝可能被 Unity 编译排除（取决于 Unity 的 asmdef 覆盖规则和编译缓存）。

---

## 五、`_C_` vs `_S_` 的目录生成规则

```csharp
// Proto2CS.cs line 244-255
if (cs.Contains('C'))  // _C_ 前缀
{
    GenerateCS(result, clientMessagePath);      // → Client/
    GenerateCS(result, serverMessagePath);      // → Server/
    GenerateCS(result, clientServerMessagePath); // → ClientServer/
}

if (cs.Contains('S'))  // _S_ 前缀
{
    GenerateCS(result, serverMessagePath);      // → Server/
    GenerateCS(result, clientServerMessagePath); // → ClientServer/
}
```

| 前缀 | Client/ | Server/ | ClientServer/ | 编译目标 |
|------|---------|---------|---------------|---------|
| `_C_` | ✅ | ✅ | ✅ | Client/Server→Assembly-CSharp, ClientServer→ET.Model |
| `_S_` | ❌ | ✅ | ✅ | Server→Assembly-CSharp, ClientServer→ET.Model |

---

## 六、ET10 为什么拆分不报错

### ET10 结构

```
cn.etetet.unitybridge/Proto/
├── UnityBridge_C_11100.proto  → 部分消息
└── UnityBridge_C_11400.proto  → 其余消息
```

两个文件都 `_C_` 前缀，各自生成 Client/Server/ClientServer 三份 → 6 文件。

### 不报错的原因

1. **ET10 没有 `[ResponseType]` 前置错误**。拆分成两个文件后，opcode 范围不重叠（11100-11399 vs 11400-11699），同文件内不碰撞。

2. **3 份拷贝问题可能被 ET10 的 asmdef 配置消解了**。ET10 的 proto 包可能有 asmdef 配置排除 Client/ 和 Server/ 目录，只有 ClientServer/ 编译进 ET.Model。需进一步验证。

3. **ET10 使用统一 Opcode 枚举**。生成的 `[Message(Opcode.Ping)]` 引用外部枚举而非 per-file 静态类，opcode 集中管理避免冲突。

### ET9 要做类似改动需要改什么

如果 ET9 要支持拆分 proto 文件：

1. **Client/ 和 Server/ 目录必须加 asmref**，指向 ET.Model，让 3 份拷贝编译到同一个程序集 → Type 去重。

2. **或者**：修改 Proto2CS，`_C_` 前缀只生成 ClientServer/ 一份拷贝（类似 ET10 的行为）。

3. **或者**：UBridge 用 `_S_` 前缀（Server），只生成 Server/ + ClientServer/ 两份，再排除 Server/ 的 Unity 编译。

4. **或者**：简单合回一个文件 + 删除 `// ResponseType`，两个问题同时消失。

---

## 七、总结

| 问题 | 原因 |
|------|------|
| `not found responseType` | UBridge 消息不该有 `ResponseType`（非网络 RPC），去掉 |
| `double map add fail` | 拆分后 Client/ 和 ClientServer/ 编译到不同程序集，相同 opcode 冲突 |
| ET10 拆分正常 | Client/Server/ 目录可能被 asmdef 排除，或 Opcode 统一枚举机制不同 |

**推荐方案：合回单文件 + 删 ResponseType**。不引入新复杂度。
