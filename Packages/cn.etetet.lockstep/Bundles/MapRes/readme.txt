MapRes：战斗/城镇地图瓦片资源（按地图懒加载，03 文档 §3.2）

目录约定：MapRes/{地图名}/
  tile_layout.json        瓦片布局+碰撞矩阵（翻译工具 DnfConfigTranslation 的 til 子命令产物）
  {imgName}.img.bytes     瓦片贴图图集（NPK 提取，多瓦片常共用一张）

training_room/ = 阿甘左训练场（MapIds.TrainingRoom，15089training.map，4 瓦片：
BH004/BH002/BH001/BH005，瓦片图集 sprite_map_village_aganzo_aganzo.img 5 帧）

tile_layout.json 字段约定（运行时消费方 LSMapViewComponentSystem / RoomSystem.InitCollision）：
  gridWidth/gridHeight   碰撞矩阵格数（基础瓦片 14 列 x 30 行，多瓦片水平拼接列数相加）
  cellSizePx             每格像素（DNF 80）
  passTypes              压平 pass type 串（gridWidth*gridHeight 个字符，行优先自上而下；'2'=可走 '0'=阻挡）
  tiles[]                { imgName（同目录图集名，不含 .img.bytes）、frame（帧号）、x/y（大图 Blit 左上角 px）}
  像素坐标 = 大图坐标，网格原点 (0,0) = 大图左上角；1px 深度 = 0.01 单位（DNF y ↔ 世界 z）
