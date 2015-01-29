using System;
using System.Diagnostics;

namespace GoapSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ActionPlanner ap = ActionPlanner.Instance;

			ap.SetPrecondition("scout", "armedwithgun", true );
			ap.SetPostcondition("scout", "enemyvisible", true );

			ap.SetPrecondition("approach", "enemyvisible", true );
			ap.SetPostcondition("approach", "nearenemy", true );

			ap.SetPrecondition("aim", "enemyvisible", true );
			ap.SetPrecondition("aim", "weaponloaded", true );
			ap.SetPostcondition("aim", "enemylinedup", true );

			ap.SetPrecondition("shoot", "enemylinedup", true );
			ap.SetPostcondition("shoot", "enemyalive", false );

			ap.SetPrecondition("load", "armedwithgun", true );
			ap.SetPostcondition("load", "weaponloaded", true );

			ap.SetPrecondition("detonatebomb", "armedwithbomb", true );
			ap.SetPrecondition("detonatebomb", "nearenemy", true );
			ap.SetPostcondition("detonatebomb", "alive", false );
			ap.SetPostcondition("detonatebomb", "enemyalive", false );

			ap.SetPrecondition("flee", "enemyvisible", true );
			ap.SetPostcondition("flee", "nearenemy", false );

			Console.WriteLine(ap.Describe());

			var startState = new WorldState();
			startState.Set(ap.FindConditionNameIndex("enemyvisible"), false );
			startState.Set(ap.FindConditionNameIndex("armedwithgun"), true );
			startState.Set(ap.FindConditionNameIndex("weaponloaded"), false );
			startState.Set(ap.FindConditionNameIndex("enemylinedup"), false );
			startState.Set(ap.FindConditionNameIndex("enemyalive"), true );
			startState.Set(ap.FindConditionNameIndex("armedwithbomb"), true );
			startState.Set(ap.FindConditionNameIndex("nearenemy"), false );
			startState.Set(ap.FindConditionNameIndex("alive"), true );

			ap.SetCost("detonatebomb", 1000 );	// make suicide more expensive than shooting.

			var goal = new WorldState();
			goal.Set(ap.FindConditionNameIndex("enemyalive"), false );
			goal.Set(ap.FindConditionNameIndex("alive"), true );
			//goap_worldstate_set( &ap, &goal, "alive", true ); // add this to avoid suicide actions in plan.

			Console.WriteLine ("Start: " + startState);
			Console.WriteLine ("Goal: " + goal);


			Stopwatch sw = new Stopwatch ();
			sw.Start ();

			for (var i = 0; i < 10000; i++) {
				var plan = AStarSharp.Plan (ap, startState, goal);
			}
			sw.Stop ();
			Console.WriteLine ("C# Elapsed: " + sw.ElapsedMilliseconds);
			sw.Reset();

//			for (var i=0; i<plan.Count;i++) {
//				Console.WriteLine (string.Format("{0}: {1}", i, plan[i]));
//			}

			sw = new Stopwatch ();
			sw.Start ();

			for (var i = 0; i < 10000; i++) {
				var states = new WorldState[16];
				var plann = new string[16];
				var plansz = 16;
				int plancost = AStar.Plan(ap, startState, goal, plann, states, ref plansz );
			}
			sw.Stop ();
			Console.WriteLine ("C Elapsed: " + sw.ElapsedMilliseconds);


//
//			Console.WriteLine ("\n==============================================\n");
//			Console.WriteLine("plancost = " + plancost );
//			Console.WriteLine ("Plan Start: " + startState); // &ap, &fr, desc, sizeof( desc ) );
//			for ( int i=0; i<plansz && i<16; ++i )
//			{
//				Console.WriteLine (string.Format("{0}: {1} - {2}", i, plan[i], states[i]));
//			}
		}
	}
}
