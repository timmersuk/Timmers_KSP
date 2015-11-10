using System;

namespace KeepFit
{
	/// <summary>
	/// Provides methods for calls of KeepFit's API functions
	/// </summary>
	public class KeepFitAPIImplementation
	{
		
		private static KeepFitAPIImplementation inst;
		
		private GameConfig gameConfig;
		
		private KeepFitAPIImplementation() { }
		
		public static KeepFitAPIImplementation instance() {
			if (inst == null) {
				inst = new KeepFitAPIImplementation();
			}
			return inst;
		}
		
		internal void setGameConfig(GameConfig gameConfig)
		{
			this.gameConfig = gameConfig;
		}
		
		private KeepFitCrewMember getCrewMember(string kerbalName) {
			if (gameConfig == null) {
				return null;
			}
			
			KeepFitCrewMember crewMember;
			if (! gameConfig.roster.crew.TryGetValue(kerbalName, out crewMember)) {
				return null;
			}
			return crewMember;
		}
		
		public Single? getFitnessLevel(string kerbalName) {
			KeepFitCrewMember crewMember = getCrewMember(kerbalName);
			return crewMember != null ? (Single?)crewMember.fitnessLevel : null;
		}
		
		public float? getFitnessGeeToleranceModifier(string kerbalName) {
			if (gameConfig == null) {
				return null;
			}
			KeepFitCrewMember crewMember = getCrewMember(kerbalName);
			return crewMember != null ? (float?)GeeLoadingCalculator.GetFitnessModifiedGeeTolerance((float)1.0, crewMember, gameConfig) :  null;
		}
		
	}
}
