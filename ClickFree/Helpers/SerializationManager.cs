using System.IO;
using System.Runtime.Serialization.Json;

namespace ClickFree.Helpers
{
    public static  class SerializationManager
    {
        #region Methods

        public static T Deserialize<T>(Stream stream) where T : class
        {
            T result = default(T);

            try
            {
                if (stream != null)
                {
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(T));

                    result = dcjs.ReadObject(stream) as T;
                }
            }
            catch
            {
            }

            return result;
        }

        #endregion
    }
}
