using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    namespace EventType
    {
        public struct BuffTime0ut
        {
            public Unit Unit;
            public long BuffId;
        }


        public struct CastStart
        {
            public long CasterId;
            public long CastConfigId;
            public long CastId;
        }

        public struct CastHit
        {
            public long CasterId;
            public long TargetId;
            public long CastId;
        }

        public struct CastFinish
        {
            public long CasterId;
            public long CastId;
        }


        public struct CastBreak
        {
            public long CasterId;
            public long CastId;
        }

        public struct BuffAdd
        {
            public Unit Unit;
            public long BuffId;
            public int BuffConfigId;
        }

        public struct BuffRemove
        {
            public Unit Unit;
            public long BuffId;
        }
        public struct BuffTick
        {
            public Unit unit;
            public long BuffId;
        }

        public struct BuffUpdate
        {
            public Unit unit;
            public long BuffId;
            public int BuffConfigId;
        }

    }


    }
