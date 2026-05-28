using GameFramework.Procedure;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Pokemon
{
    public class MyProcedureMain : MyProcedureBase
    {
        private Player mPlayer = null;

        // OnInit is called when the procedure is initialized.
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            mPlayer = new Player();
            mPlayer.Initialize();
        }

        // OnEter is called when the procedure is entered. 
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("ProcedureMain: Enter");
        }

        // Onupdate is called every frame when the procedure is running.
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (mPlayer != null)
            {
                mPlayer.Update();
            }
        }

        // OnLeave is called when the procedure is left.
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            //mPlayer?.Shutdown();
            mPlayer = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        // OnDestroy is called when the procedure is destroyed.
        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }
    }
}