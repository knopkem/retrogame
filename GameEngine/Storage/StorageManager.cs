using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using GameEngine.DebugTools;

namespace GameEngine.Storage
{
    public class StorageManager
    {
        private const string FileName = "StorageData.xml";


        public List<StorageData> LoadStorageData()
        {
            var storageData = new List<StorageData>();
            TextReader reader = null;
            try
            {
                IsolatedStorageFile isoStorage = GetUserStoreAsAppropriateForCurrentPlatform();
                string[] files = isoStorage.GetFileNames();
                foreach (string file in files)
                    TraceLog.WriteWarning(file);
                if (isoStorage.FileExists(FileName))
                {
                    IsolatedStorageFileStream file = isoStorage.OpenFile(FileName, FileMode.Open);
                    reader = new StreamReader(file);

#if WINDOWS
                    var xs = new XmlSerializer(typeof (List<StorageData>));
                    storageData.AddRange((List<StorageData>) xs.Deserialize(reader));
                    reader.Close();
#endif
                }
            }
            catch (Exception e)
            {
                TraceLog.WriteWarning("Exception in StorageManager.LoadStorageData" + e.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
            return storageData;
        }


        public void SaveStorageData(List<StorageData> storageData)
        {
            TextWriter writer = null;
            try
            {
                IsolatedStorageFile isoStorage = GetUserStoreAsAppropriateForCurrentPlatform();
                IsolatedStorageFileStream file = isoStorage.OpenFile(FileName, FileMode.Create);
                writer = new StreamWriter(file);
#if WINDOWS
                var xs = new XmlSerializer(typeof (List<StorageData>));
                xs.Serialize(writer, storageData);
                writer.Close();
#endif
            }
            catch (Exception e)
            {
                TraceLog.WriteWarning("Exception in StorageManager.SaveStorageData" + e.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Dispose();
            }
        }

        public IsolatedStorageFile GetUserStoreAsAppropriateForCurrentPlatform()
        {
#if WINDOWS
            return IsolatedStorageFile.GetUserStoreForDomain();
#else
        return IsolatedStorageFile.GetUserStoreForApplication();
#endif
        }
    }
}