using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TypeMaster.Service;

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
            {typeof(List<WikipediaPageInfo>), _wikipediaPageInfosFilePath},
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

        if (!SavableData.TryGetValue(typeof(T), out string? path) || path == null)
            return;
        try
        {
            string json = JsonConvert.SerializeObject(data);
            string encryptedJson = _cryptographyService.Encrypt(json);
            if (!File.Exists(path))
                File.Create(path).Close();
            using (StreamWriter sw = new StreamWriter(path, false))
                sw.Write(encryptedJson);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public async Task<T?> GetDataAsync<T>()
    {
        if (!CheckIfPathExisted() || !File.Exists(_wikipediaPageInfosFilePath) || !SavableData.TryGetValue(typeof(T), out string? path))
            return default;
        
        try
        {
            using (StreamReader sr = new StreamReader(path!))
            {
                string encryptedJson = await sr.ReadToEndAsync();
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

    public async Task SaveDataAsync<T>(T data)
    {
        CheckIfPathExisted();

        if (!SavableData.TryGetValue(typeof(T), out string? path) || path == null)
            return;
        try
        {
            string json = JsonConvert.SerializeObject(data);
            string encryptedJson = _cryptographyService.Encrypt(json);
            if(!File.Exists(path))
                File.Create(path).Close();
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                await sw.WriteAsync(encryptedJson);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public async IAsyncEnumerable<T> GetDataCollectionAsync<T>()
    {
        if (!CheckIfPathExisted() || !File.Exists(_wikipediaPageInfosFilePath))
            yield break;

        string encryptedJson;
        try
        {
            using (StreamReader reader = new StreamReader(_wikipediaPageInfosFilePath))
            {
                encryptedJson = await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            yield break;
        }

        string decryptedJson = _cryptographyService.Decrypt(encryptedJson);
        using (JsonTextReader jsonReader = new JsonTextReader(new StringReader(decryptedJson)))
        {
            T? pageInfo;
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    JObject obj = await JObject.LoadAsync(jsonReader);
                    if ( (pageInfo = JsonConvert.DeserializeObject<T>(obj.ToString())) != null)
                        yield return pageInfo;
                }
            }
        }
        yield break;
    }
}
