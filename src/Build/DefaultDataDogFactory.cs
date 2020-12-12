using PipServices3.Commons.Refer;
using PipServices3.Components.Build;
using PipServices3.DataDog.Log;

namespace PipServices3.DataDog.Build
{
    /// <summary>
    /// Creates DataDog components by their descriptors.
    /// </summary>
    public class DefaultDataDogFactory : Factory
    {
	    public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "datadog", "default", "1.0");
	    public static Descriptor DataDogLoggerDescriptor = new Descriptor("pip-services", "logger", "datadog", "*", "1.0");

        /// <summary>
        /// Create a new instance of the factory.
        /// </summary>
        public DefaultDataDogFactory()
        {
            RegisterAsType(DataDogLoggerDescriptor, typeof(DataDogLogger));
        }
    }
}
