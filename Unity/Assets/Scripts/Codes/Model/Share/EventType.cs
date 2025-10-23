using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    namespace EventType
    {
        public struct BuffTimeOut
        {
            public Unit Unit;
            public long BuffId;
        }


        public struct PlayerUnitTransferToRealMap
        {
            public Unit Unit;
            public long SceneInstanceId;
        }

    }
}
