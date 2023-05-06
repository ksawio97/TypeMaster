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

        public DataSaveLoadService()
        {
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypeMaster");
            _filePath = Path.Combine(_folderPath, "data.json");

            CheckIfPathExists();
        }

        private void CheckIfPathExists()
        {
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
            if (!File.Exists(_filePath))
                File.Create(_filePath);
        }

        public HashSet<WikipediaPageInfo> GetWikipediaPageInfos()
        {
            CheckIfPathExists();

            try
            {
                using (StreamReader sr = new StreamReader(_filePath))
                {
                    string json = sr.ReadToEnd();
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
            CheckIfPathExists();

            try
            {
                string json = JsonConvert.SerializeObject(wikipediaPageInfos);
                using (StreamWriter sw = new StreamWriter(_filePath, false))
                {
                    sw.Write(json);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
