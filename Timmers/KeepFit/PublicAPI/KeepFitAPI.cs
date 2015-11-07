using System;

namespace KeepFit
{
	/// <summary>
	/// This class provides API methods to get various values provided by KeepFit mod 
	/// </summary>
	public class KeepFitAPI : ksp_intermod_api.APIReflectionCaller
	{
		public KeepFitAPI()
		{
		}
		
		protected override string getImplementationName() {
			return "KeepFit.KeepFitAPIImplementation";
		}
		
		public Single? getFitnessLevel(string kerbalName) {
			return isInitialized() ? (Single?)invokeMethod("getFitnessLevel", new object[]{kerbalName}) : null;
		}
		
		public float? getFitnessGeeToleranceModifier(string kerbalName) {
			return isInitialized() ? (float?)invokeMethod("getFitnessGeeToleranceModifier", new object[]{kerbalName}) : null;
		}
	}
}
