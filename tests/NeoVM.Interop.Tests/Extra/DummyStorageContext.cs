using System.Collections.Generic;

namespace NeoVM.Interop.Tests.Extra
{
    public class DummyStorageContext
    {
        /// <summary>
        /// Script hash
        /// </summary>
        public readonly byte[] ScriptHash;
        /// <summary>
        /// Id
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Storage
        /// </summary>
        public readonly Dictionary<string, byte[]> Storage = new Dictionary<string, byte[]>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="scriptHash">Script hash</param>
        public DummyStorageContext(string id, byte[] scriptHash)
        {
            ScriptHash = scriptHash;
            Id = id;
        }
    }
}