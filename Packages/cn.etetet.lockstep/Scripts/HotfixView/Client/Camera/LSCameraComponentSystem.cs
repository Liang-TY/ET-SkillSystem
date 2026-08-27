using UnityEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(LSCameraComponent))]
	[FriendOf(typeof(LSCameraComponent))]
	[FriendOf(typeof(LSCollisionComponent))]   // 读 OriginX/OriginZ 钳相机边界（ET0002）
	public static partial class LSCameraComponentSystem
	{
		[EntitySystem]
		private static void Awake(this LSCameraComponent self)
		{
			self.Camera = Camera.main;
			self.Camera.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
		}
		
		[EntitySystem]
		private static void LateUpdate(this LSCameraComponent self)
		{
			// 摄像机每帧更新位置
			Room room = self.GetParent<Room>();
			if (room.IsReplay)
			{
				if (Input.GetKeyDown(KeyCode.Tab))
				{
					++self.index;
					self.MyUnitView = new LSUnitView();
				}
			}

			LSUnitView lsUnit = self.MyUnitView;
			if (lsUnit == null)
			{
				long id = room.IsReplay? room.PlayerIds[self.index % room.PlayerIds.Count] : room.GetParent<Scene>().GetComponent<PlayerComponent>().MyId;
				self.MyUnitView = room.GetComponent<LSUnitViewComponent>().GetChild<LSUnitView>(id);
			}

			if (lsUnit == null)
			{
				return;
			}

			Vector3 pos = lsUnit.Transform.position;
			// 相机跟随（2026-08-28）：角色固定屏幕中央 → 眼睛能聚焦；+0.5 让角色居中（脚底上方 0.5 单位）。
			Vector3 target = new Vector3(pos.x, pos.y + 0.5f, 0f);

			// 边界钳制：读碰撞组件 OriginX/OriginZ（视觉半宽/半高），clamp 到地图内再 snap 到像素格
			LSCollisionComponent collision = room.LSWorld?.GetComponent<LSCollisionComponent>();
			if (collision != null)
			{
				self.Transform.position = CameraClampHelper.ClampToMap(
					target, (float)collision.OriginX, (float)collision.OriginZ, self.Camera);
			}
			else
			{
				self.Transform.position = new Vector3(
					Mathf.Round(target.x * 100f) / 100f,
					Mathf.Round(target.y * 100f) / 100f,
					self.Transform.position.z);
			}
		}
	}
}
