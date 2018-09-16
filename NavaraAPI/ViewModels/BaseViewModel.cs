using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavaraAPI.ViewModels
{
    public abstract class BaseViewModel
    {
        public Guid? ID { get; set; }
        public byte[] RowVersion  { get; set; }
    }
}
