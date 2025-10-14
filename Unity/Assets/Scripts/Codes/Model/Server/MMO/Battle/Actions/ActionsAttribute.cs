using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class ActionsAttribute : BaseAttribute
    {
        public int ActionsType { get; }







        public ActionsAttribute(int actionsType) 
        {
        
            this.ActionsType = actionsType;
        }
    }
}
