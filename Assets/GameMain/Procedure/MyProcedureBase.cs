using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace Monster
{
    public abstract class MyProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        public abstract bool UseNativeDialog { get; }  
    }
}
