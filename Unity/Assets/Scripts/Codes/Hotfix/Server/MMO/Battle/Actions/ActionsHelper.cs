using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    [FriendOfAttribute(typeof(ET.Server.Actions))]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    [FriendOfAttribute(typeof(ET.Server.Buff))]
    public static class ActionsHelper
    {
        public static Actions CreateActions(this ActionsTempComponent self, int configId)
        {
            return self.AddChild<Actions, int>(configId);
        }

        public static Actions CreateActions(this BulletComponent self, int configId, Unit owner,Unit caster,
            ActionsRunType actionsRunType,bool autoRun = true,bool autoDispose = true)
        {
            Actions actions = self.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = caster;
            actions.Owner = owner;
            RunActions(actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }
            

            return actions;
        }


        public static Actions CreateActions(this Buff buff, int configId,
ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = buff.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Owner = buff.Owner;
            RunActions(actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }
            return actions;
        }

        public static Actions CreateActions(this Cast cast, int configId, Unit owner,
            ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = cast.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = cast.Caster;
            actions.Owner = owner;
            RunActions(actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }
            return actions;
        }


        public static void RunActions(Actions actions, ActionsRunType actionsRunType,
            bool autoRun = true,bool autoDispose = true)
        {
            if (autoRun)
            {
                if (autoDispose)
                {

                    //当actions对象实现了IDisposable接口时，using语句会在代码块执行结束后自动调用Dispose()方法释放资源（如文件句柄、数据库连接等），无需手动处理。这是using最常见的用途。

                    using (actions)
                    {
                        RunActionsInner(actions, actionsRunType);
                    }
                }
            }
            else
            {
                RunActionsInner(actions,actionsRunType);
            }
        }


        public static void RunActionsInner(Actions actions, ActionsRunType actionsRunType)
        {
            IActions actionsHandle = ActionsDispatcheerComponent.Instance.Get(actions.Config.Type);
            if (actionsHandle != null)
            {
                Log.Error($"Error! Actions type not found, UnitID: {actions.Owner?.Id }, ActionsconfigID: {actions.ConfigId}");
                actions.Dispose();
                return;
            }
            actionsHandle.Run(actions,actionsRunType);
        }

    }

}
