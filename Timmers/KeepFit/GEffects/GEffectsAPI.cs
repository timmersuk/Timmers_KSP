using System;

namespace G_Effects
{
	/// <summary>
	/// This class provides API methods to get various values provided by G-Effects mod 
	/// </summary>
	[KSPAddon(KSPAddon.Startup.MainMenu, false)] 
	public class GEffectsAPI : ksp_intermod_api.APIReflectionCaller
	{
		/// <summary>
		/// Returns a name of the class that provides API implementation on G-Effects side 
		/// </summary>
		/// <returns>
		/// Fully qualified name of the API providing class
		/// </returns>
		protected override string getImplementationName() {
			return "G_Effects.GEffectsAPIImplementation";
		}
		
		/// <summary>
		/// Checks if data on G effects on specified kerbal are available
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal to check 
		/// </param>
		/// <returns>
		/// true if data are available, false otherwise
		/// </returns>
		public bool isKerbalGStateAvailable(string kerbalName) {
			return isInitialized() && (bool)invokeMethod("isKerbalGStateAvailable", new object[]{kerbalName});
		}
		
		/// <summary>
		/// Gets the value of longitudal G loading of the specified kerbal 
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// G load (in G) of the kerbal in downward direction (negative in case of upward direction)
		/// or null if the value is unavailable
		/// </returns>
		public double? getDownwardG(string kerbalName) {
			return isInitialized() ? (double?)invokeMethod("getDownwardG", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Gets the value of lateral G loading of the specified kerbal 
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// G load (in G) of the kerbal in forward direction (negative in case of backward direction)
		/// or null if the value is unavailable
		/// </returns>
		public double? getForwardG(string kerbalName) {
			return isInitialized() ? (double?)invokeMethod("getForwardG", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Checks if the specified kerbal has started the anti-G straining maneuver 
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// true if the kerbal has started AGSM, false otherwise
		/// or null if the value is unavailable
		/// </returns>
		public bool? isAGSMStarted(string kerbalName) {
			return isInitialized() ? (bool?)invokeMethod("isAGSMStarted", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Gets the number of times the specified kerbal needs to take breath while on rest
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// A positive number of times for the kerbal to take breath
		/// or null if the value is unavailable
		/// </returns>
		public int? getBreathNeeded(string kerbalName) {
			return isInitialized() ? (int?)invokeMethod("getBreathNeededed", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Checks if the specified kerbal is in state of G induced loss of consciousness, 
		/// including but not limited to the critical state and the about-to-die state
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// true if the kerbal is in G-LOC condition, false otherwise
		/// or null if the value is unavailable
		/// </returns>
		public bool? isGLocCondition(string kerbalName) {
			return isInitialized() ? (bool?)invokeMethod("isGLocCondition", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Checks if the specified kerbal is in state of critical condition and on his way to death of an excessive G load, 
		/// including but not limited to the about to die in next millisecond state
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// true if the kerbal is in critical condition, false otherwise
		/// or null if the value is unavailable
		/// </returns>
		public bool? isCriticalCondition(string kerbalName) {
			return isInitialized() ? (bool?)invokeMethod("isCriticalCondition", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Checks if the specified kerbal is in state very close to death of an excessive G load in next milliseconds, 
		/// literally his death is imminent in this state but he wasn't removed from his vessel and roster yet
		/// </summary>
		/// <param name="kerbalName">
		/// Name of a kerbal
		/// </param>
		/// <returns>
		/// true if the kerbal is in imminent death of excessive G load, false otherwise
		/// or null if the value is unavailable
		/// </returns>
		public bool? isDeathCondition(string kerbalName) {
			return isInitialized() ? (bool?)invokeMethod("isDeathCondition", new object[]{kerbalName}) : null;
		}
		
		/// <summary>
		/// Gets the severity of G load 
		/// </summary>
		/// <param name="kerbalName"></param>
		/// <returns></returns>
		public float? getSeverity(string kerbalName) {
			return isInitialized() ? (float?)invokeMethod("getSeverity", new object[]{kerbalName}) : null;
		}
	}
	
}
