using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TypeMaster.Service
{
    public class DataSaveLoadService
    {
        private readonly string _folderPath;
        private readonly string _wikipediaPageInfosFilePath;
        private readonly string _settingsFilePath;

        readonly CryptographyService _cryptographyService;

        public readonly Dictionary<Type, string> SavableData;

        public DataSaveLoadService(CryptographyService cryptographyService)
        {
            _cryptographyService = cryptographyService;

            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypeMaster");
            _wikipediaPageInfosFilePath = Path.Combine(_folderPath, "data.json.enc");
            _settingsFilePath = Path.Combine(_folderPath, "settings.json.enc");

            SavableData = new Dictionary<Type, string>
            {
                {typeof(HashSet<WikipediaPageInfo>), _wikipediaPageInfosFilePath},
                {typeof(Settings), _settingsFilePath}
            };
        }

        private bool CheckIfPathExisted()
        {
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
                return false;
            }
            return true;
        }

        public T? GetData<T>()
        {
            if (!CheckIfPathExisted() || !File.Exists(_wikipediaPageInfosFilePath) || !SavableData.TryGetValue(typeof(T), out string? path))
                return default;
            
            try
            {
                using (StreamReader sr = new StreamReader(path!))
                {
                    string encryptedJson = sr.ReadToEnd();
                    string json = _cryptographyService.Decrypt(encryptedJson);

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
                string encryptedJson = _cryptographyService.Encrypt(json);
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
