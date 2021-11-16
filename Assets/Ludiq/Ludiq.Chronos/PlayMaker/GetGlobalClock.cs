#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Stores a global clock on the timekeeper in a variable.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timekeeper.Clock")]
	public class GetGlobalClock : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmObject storeValue;

		[RequiredField]
		public FsmString globalClockKey;

		public override void Reset()
		{
			storeValue = null;
			globalClockKey = null;
		}

		public override void OnEnter()
		{
			storeValue.Value = Timekeeper.instance.Clock(globalClockKey.Value);
		}
	}
}

#endif
