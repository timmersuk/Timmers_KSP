using System;
using System.Reflection;
using System.Linq;

namespace ksp_intermod_api
{
	/// <summary>
	/// This class is base class for API-using classes and provides reflection methods to find an instance of the specified type and invoke its methods
	/// </summary>
	public abstract class APIReflectionCaller
	{
		
		object implementation; //Instance of an API providing class found on initialization
						
		/// <summary>
		/// Must be implemented by a child class.
		/// It is called automatically when initialize() method is called.		
		/// </summary>
		/// <returns>
		/// A fully qualified name of API providing class of the plugin that provides API.
		/// The class must have a public static method instance() that returns its instance.
		/// </returns>
		protected abstract string getImplementationName();
		
		/// <summary>
		/// Initializes the API calling instance (this) by trying to find an instance of a class with a fully qualified name 
		/// provided by getImplementationName() method. The class must have a public static method instance() that returns its instance.
		/// API calling instance remains uninitialized if the method fails.  
		/// </summary>
		/// <returns>
		/// true if initialized
		/// </returns>
		public bool initialize() {
			if (! isInitialized()) {
				implementation = findImplementation(this, getImplementationName());
			}
			return isInitialized();
		}
		
		/// <summary>
		/// Checks if the API calling instance (this) has been successfully initialized.
		/// </summary>
		/// <returns>
		/// true if initialized
		/// </returns>
		public bool isInitialized() {
			return implementation != null;
		}
		
		
		/// <summary>
		/// Finds an instance of API implementing class by a fully qualified name specified in implementationName parameter.
		/// The class must have a public static method instance() that returns its instance.
		/// </summary>
		/// <param name="caller">
		/// An instance that calls the method's invocation, required by reflection calls 
		/// </param>
		/// <param name="implementationName">
		/// A fully qualified name of API implementing class of the plugin that provides API 
		/// </param>
		/// <returns></returns>
		protected static object findImplementation(object caller, string implementationName) {
			Type type = AssemblyLoader.loadedAssemblies
				.SelectMany(a => a.assembly.GetExportedTypes())
				.SingleOrDefault(t => t.FullName.Equals(implementationName));
			if (type == null) {
				return (object)null;
			}
			
			MethodInfo method = type.GetMethod("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			
			if (method == null) {
				return (object)null;
			}
			
			return (object)method.Invoke(caller, null);
			
		}
		
		/// <summary>
		/// Invokes specified method of API providing instance found by initialize() method. 
		/// </summary>
		/// <param name="methodName">
		/// Name of the method to invoke
		/// </param>
		/// <param name="parameters">
		/// An array of the method parameter values
		/// </param>
		/// <returns>
		/// Resulting that has been returned by the specified method
		/// </returns>
		public object invokeMethod(string methodName, object[] parameters) {
			Type[] types = null;
			if (parameters != null) {
				types = new Type[parameters.Count()];
				for (int i=0; i<parameters.Count(); i++) {
					types[i] = parameters[i].GetType();
				}
			}
			MethodInfo method = implementation.GetType().GetMethod(methodName, types);
			if (method == null) {
				throw new MissingMethodException();
			}
			return method.Invoke(implementation, parameters);
		}
		
	}
}
