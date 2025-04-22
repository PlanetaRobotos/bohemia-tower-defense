using System.IO;
using Features.Data.Declaration;
using UnityEngine;

namespace Features.Data.Implementation.Savers
{
	/// <summary>
	///     Json implementation of file saver
	/// </summary>
	public class JsonSaver<T> : FileSaver<T> where T : IDataStore
    {
        public JsonSaver(string filename)
            : base(filename)
        {
        }

        /// <summary>
        ///     Save the specified data store
        /// </summary>
        public override void Save(T data)
        {
            var json = JsonUtility.ToJson(data);

            using (var writer = GetWriteStream())
            {
                writer.Write(json);
            }
        }

        /// <summary>
        ///     Load the specified data store
        /// </summary>
        public override bool Load(out T data)
        {
            if (!File.Exists(m_Filename))
            {
                data = default;
                return false;
            }

            using (var reader = GetReadStream())
            {
                data = JsonUtility.FromJson<T>(reader.ReadToEnd());
            }

            return true;
        }
    }
}