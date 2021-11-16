#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Sets the time scale on a global clock smoothly over time.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Clock.LerpTimeScale")]
	public class ScaleGlobalClockTimeSmoothly : FsmStateAction
	{
		[RequiredField]
		public FsmString globalClockKey;

		[RequiredField]
		public FsmFloat timeScale;

		[RequiredField]
		public FsmFloat duration;

		public bool steady;

		public override void Reset()
		{
			globalClockKey = null;
			timeScale = null;
			duration = null;
			steady = false;
		}

		public override void OnEnter()
		{
			DoAction();
		}

		private void DoAction()
		{
			Timekeeper.instance.Clock(globalClockKey.Value).LerpTimeScale(timeScale.Value, duration.Value, steady);
		}
	}
}

#endif
