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
        private readonly string _filePath;
        private CryptographyService _cryptographyService;

        public DataSaveLoadService(CryptographyService cryptographyService)
        {
            _cryptographyService = cryptographyService;

            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypeMaster");
            _filePath = Path.Combine(_folderPath, "data.json.enc");
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

        public HashSet<WikipediaPageInfo> GetWikipediaPageInfos()
        {
            if(!CheckIfPathExisted() || !File.Exists(_filePath))
                return new HashSet<WikipediaPageInfo>();

            try
            {
                using (StreamReader sr = new StreamReader(_filePath))
                {
                    string encryptedJson = sr.ReadToEnd();
                    string json = _cryptographyService.Decrypt(encryptedJson);

                    return JsonConvert.DeserializeObject<HashSet<WikipediaPageInfo>>(json) ?? new HashSet<WikipediaPageInfo>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return new HashSet<WikipediaPageInfo>();
        }

        public void SaveWikipediaPageInfos(HashSet<WikipediaPageInfo> wikipediaPageInfos)
        {
            CheckIfPathExisted();

            try
            {
                string json = JsonConvert.SerializeObject(wikipediaPageInfos);
                string encryptedJson = _cryptographyService.Encrypt(json);
                if(!File.Exists(_filePath))
                    File.Create(_filePath).Close();
                using (StreamWriter sw = new StreamWriter(_filePath, false))
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
