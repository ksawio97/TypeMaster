using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TypeMaster.Service
{
    public class DataSaveLoadService
    {
        private readonly string FolderPath;
        private readonly string WikipediaPageInfosFilePath;
        private readonly string SettingsFilePath;

        readonly CryptographyService CryptographyService;

        public readonly Dictionary<Type, string> SavableData;

        public DataSaveLoadService(CryptographyService cryptographyService)
        {
            CryptographyService = cryptographyService;

            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypeMaster");
            WikipediaPageInfosFilePath = Path.Combine(FolderPath, "data.json.enc");
            SettingsFilePath = Path.Combine(FolderPath, "settings.json.enc");

            SavableData = new Dictionary<Type, string>
            {
                {typeof(HashSet<WikipediaPageInfo>), WikipediaPageInfosFilePath},
                {typeof(Settings), SettingsFilePath}
            };
        }

        private bool CheckIfPathExisted()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                return false;
            }
            return true;
        }

        public T? GetData<T>()
        {
            if (!CheckIfPathExisted() || !File.Exists(WikipediaPageInfosFilePath) || !SavableData.TryGetValue(typeof(T), out string? path))
                return default;
            
            try
            {
                using (StreamReader sr = new StreamReader(path!))
                {
                    string encryptedJson = sr.ReadToEnd();
                    string json = CryptographyService.Decrypt(encryptedJson);

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return default;
        }

        public void SaveData<T>(T data)
        {
            CheckIfPathExisted();

            if (!SavableData.TryGetValue(typeof(T), out string path))
                return;
            try
            {
                string json = JsonConvert.SerializeObject(data);
                string encryptedJson = CryptographyService.Encrypt(json);
                if(!File.Exists(path))
                    File.Create(path).Close();
                using (StreamWriter sw = new StreamWriter(path, false))
                {
                    sw.Write(encryptedJson);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
