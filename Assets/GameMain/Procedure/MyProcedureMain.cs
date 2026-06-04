using GameFramework.Procedure;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    public class MyProcedureMain : MyProcedureBase
    {
        private RPGGame mGame = null;

        // OnInit is called when the procedure is initialized.
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            //mPlayer.Initialize();
        }

        // OnEter is called when the procedure is entered. 
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("ProcedureMain: Enter");

            mGame = new RPGGame();

            if (mGame == null)
            {
                Log.Info("mGame is null");
            }

            mGame.Initialize();
        }

        // Onupdate is called every frame when the procedure is running.
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (mGame != null)
            {
                mGame.Update(elapseSeconds, realElapseSeconds);
            }
        }

        // OnLeave is called when the procedure is left.
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            //mPlayer?.Shutdown();
            mGame = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        // OnDestroy is called when the procedure is destroyed.
        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        public override bool UseNativeDialog => false;
    }
}