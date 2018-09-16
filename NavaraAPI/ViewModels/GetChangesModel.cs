using Ninject;
using NavaraAPI;
using SmartLifeLtd.ViewModels;
using System;
using System.Collections.Generic;

namespace NavaraAPI.Models
{
    public class GetChangesModel
    {
        public DateTime? LastUpdate { get; set; }
        public List<Guid> RecoredIDs { set; get; }
    }

    public class ChangesModel
    {
        public object Inserted { set; get; }
        public object Updated { set; get; }
        public object Deleted { set; get; }
    }
}
