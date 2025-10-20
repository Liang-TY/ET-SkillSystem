using ET.Server;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace ET.Client
{
    public static class ParticleEffectHelper
    {
        public static Unit CreateParticle(Unit target, int configId)
        {
            ParticleEffectConfig config = ParticleEffectConfigCategory.Instance.Get(configId);
            string name = config.PrefabName;
            ResourcesComponent.Instance.LoadBundle($"{name}.unity3d");
            GameObject particleGame0biectPrefab = (GameObject)ResourcesComponent.Instance.GetAsset($"{name}.unity3d", name);
            GameObject particleGame0bject = UnityEngine.Object.Instantiate(particleGame0biectPrefab);
            if (config.IsFollowUnit != 0)
            {
                particleGame0bject.transform.SetParent(target.GetComponent<GameObjectComponent>().GameObject.transform, false);
            }
            else
            {
                particleGame0bject.transform.SetParent(GlobalComponent.Instance.Unit, false);
            }

            Unit particleUnit = UnitFactory.CreateParticleUnit(target.DomainScene());
            particleUnit.AddComponent<GameObjectComponent>().GameObject = particleGame0bject;

            particleGame0bject.transform.localPosition = new Vector3(config.PosX, config.PosY, config.PosZ);
            particleGame0bject.transform.localScale = new Vector3(config.ScaleX, config.ScaleY, config.ScaleZ);
            OutDurationTime(particleUnit, config.TotalTime).Coroutine();
            return particleUnit;
        }

        public static async ETTask OutDurationTime(Unit unit,float time)
        {
            if (time <= 0)
            {
                return;
            }

            long instanceId = unit.InstanceId;
            await TimerComponent.Instance.WaitAsync((long)time);
            if (unit.InstanceId != instanceId)
            {
                return;
            }
            unit.DomainScene()?.GetComponent<UnitComponent>()?.Remove(unit.Id);
        }


    }
}
