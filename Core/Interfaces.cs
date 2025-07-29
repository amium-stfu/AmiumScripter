using AmiumScripter.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Core
{

    public interface IClass
    {
        string InstanceName { get; }
        void Destroy();
    }
    [SupportedOSPlatform("windows")]
    public abstract class ClassBase : IClass
    {

        public string InstanceName { get; init; }

        protected ClassBase(string instanceName)
        {
            InstanceName = instanceName;
            Logger.Log($"[ClassBase] Register {GetType().Name} ({InstanceName})");
            ClassRuntimeManager.Register(this); // immer automatisch!
        }
        public abstract void Destroy();
    }

}
