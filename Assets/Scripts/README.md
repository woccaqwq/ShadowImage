# 脚本结构与调参位置

## 运行时组件

| 文件 | 职责 | 常改参数 / 挂载位置 |
| --- | --- | --- |
| Player/FirstPersonPlayer.cs | 移动、重力、碰撞、玩家控制协调 | Player：移速、眼高、重力 |
| Player/PlayerLook.cs | 鼠标转向、俯仰限制、鼠标捕获 | Player：灵敏度、俯仰限制 |
| Interaction/PuzzleInteraction.cs | 选取、操作输入、交互状态与取消恢复 | PuzzleInteraction：旋转步长、速度、交互距离 |
| Interaction/PuzzlePiece.cs | 部件类型、姿态快照、模型顶点缓存、碰撞状态 | 每个可拾取部件 |
| Interaction/HeldItemPresentation.cs | 拾取/放置动画、缩放、悬浮、手臂动作 | PuzzleInteraction：时长、悬浮偏移/幅度/频率、持物比例 |
| Puzzle/PuzzlePlacementRules.cs | 操作平面射线、完整投影轮廓、槽位匹配 | PuzzleInteraction：操作平面、光源、有效区域、容差、槽位列表 |
| Puzzle/PuzzleSlot.cs | 目标姿态、允许部件/类型、预留与正式占用 | Slot_*：Transform 就是目标姿态 |
| UI/InteractionHUD.cs | 准星颜色、操作说明、状态提示 | PuzzleInteraction：关联交互组件 |

编辑器脚本 `Editor/ConceptRoomBuilder.cs` 负责重建白模、绑定上述组件、保存场景和预览。它会重建 SampleScene 的全部对象；日常调整组件参数不需要运行该工具。

## 状态与数据约定

- 交互状态：Empty → PickingUp → Holding → Placing → Empty。
- 单件占用：预览不占位；确认放置后 Reserve；动画结束后 Commit；取回 Release。
- 取消/组件禁用时停止动画、撤销预留，恢复抓取前姿态、尺寸、碰撞与原槽位。
- 悬浮缩放只影响表现。规则通过缓存的原尺寸顶点和目标矩阵计算，不移动真实物体。
- `allowedType` 为空时只接受 `allowedPiece`；填写类型时接受同类型的 PuzzlePiece。
- 当前关卡仍使用原始摆放姿态作为参考解；正式关卡可移动 Slot_*，与部件初始姿态独立设置。
- 场景已配置显式引用，运行时不再按 Puzzle_Cube 等名称查找部件。新增部件时挂 PuzzlePiece，添加 PuzzleSlot，并加入规则组件的 slots。
- 新增几何体应保持模型网格可读；当前投影判定不支持变形蒙皮模型。

## 回归检查

1. 空手/持物时均可 WASD 移动、鼠标转向，重新捕获鼠标的点击不拾取物体。
2. 拾取期间连续点击不重复执行；持物悬浮，Q/E、滚轮按步长旋转。
3. 合法空处放下保持原尺寸；槽位容差内吸附，动画结束才正式占用。
4. 已归位部件取回释放槽位；越界或 Esc 恢复原姿态和占用。
5. 拾取/放置途中禁用交互组件，不残留缩小模型、禁用碰撞体或预留槽位。
6. 转身背对操作平面后点击，物体退回；关闭/重开场景后引用完整。

本次已执行全部脚本编译、场景本地引用/GUID/父子关系检查；上述 Play 模式检查尚待执行。

