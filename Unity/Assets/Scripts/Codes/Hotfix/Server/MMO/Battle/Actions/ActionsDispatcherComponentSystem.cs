using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class ActionsDispatcherComponentAwakeSystem:AwakeSystem<ActionsDispatcheerComponent>
    {
        protected override void Awake(ActionsDispatcheerComponent self)
        {
            ActionsDispatcheerComponent.Instance = self;
            self.Load();
        }

    }

    public class ActionsDispatcherComponentLoadSystem : LoadSystem<ActionsDispatcheerComponent>
    {
        protected override void Load(ActionsDispatcheerComponent self)
        {
            self.Load();
        }

    }

    public class ActionsDispatcherComponentDestroySystem : DestroySystem<ActionsDispatcheerComponent>
    {
        protected override void Destroy(ActionsDispatcheerComponent self)
        {
            ActionsDispatcheerComponent.Instance = null;
            self.Dict.Clear();
        }

    }
    [FriendOfAttribute(typeof(ET.Server.ActionsDispatcheerComponent))]

    public static class ActionsDispatcherComponentSystem
    {
        public static void Load(this ActionsDispatcheerComponent self)
        {
            self.Dict.Clear();
            var types = EventSystem.Instance.GetTypes(typeof(ActionsAttribute));


            foreach (Type type in types)
            {
                var attrs = type.GetCustomAttributes(typeof(ActionsAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                ActionsAttribute actionsAttribute = attrs[0] as ActionsAttribute;
                object obj = Activator.CreateInstance(type);
                IActions iActions = obj as IActions;
                if (iActions == null)
                {
                    throw new Exception($"class: {type.Name} not inherit from IActions");
                }

                self.Dict[actionsAttribute.ActionsType] = iActions;
            }




        }


        public static IActions Get(this ActionsDispatcheerComponent self, int type)
        {
            if (self.Dict.TryGetValue(type, out IActions iActions))
            {
                return iActions;
            }

            return null;
        }



    }
}
