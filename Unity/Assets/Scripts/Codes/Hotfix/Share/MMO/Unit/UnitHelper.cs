using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ET
{


    public static class UnitHelper
    {
        public static void SetRotation(this Unit unit, quaternion rotation)
        {
            if ((unit.GetComponent<NumericComponent>()?.GetAsInt(NumericType.ForbidRotation) ?? 0) > 0)
            {
                return;
            }

            unit.Rotation = rotation;
        }

    }

}
