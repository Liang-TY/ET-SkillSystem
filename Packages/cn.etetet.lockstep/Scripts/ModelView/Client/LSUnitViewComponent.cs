using UnityEngine;

namespace ET
{
	[ComponentOf(typeof(Room))]
	public class LSUnitViewComponent: Entity, IAwake, IUpdate, IDestroy
	{
		public EntityRef<LSUnitView> myUnitView;

		/// <summary>Unit2D 预制体缓存（InitAsync 载入；Update 差分新增单位视图时复用）</summary>
		public GameObject UnitPrefab;
	}
}
