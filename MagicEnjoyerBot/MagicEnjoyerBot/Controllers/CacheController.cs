using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Controllers
{
    public static class CacheController
    {
        static string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        static string FolderPath = Path.Combine(systemPath, "MagicEnjoyerBot");
        static string SavePath = Path.Combine(systemPath, "MagicEnjoyerBot\\MagicEnjoyerBotCache.txt");

        public static InfoCache infoCache = new InfoCache();

        public static void Initialize()
        {
            Console.WriteLine(systemPath);
            //Create folder
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            //Create File
            if (!File.Exists(SavePath))
            {
                FileStream newFile = File.Create(SavePath);
                newFile.Close();
                //SaveList();
            }
            else
            {
                try
                {
                    string storedList = File.ReadAllText(SavePath);

                    infoCache = JsonConvert.DeserializeObject<InfoCache>(storedList);

                    if(infoCache == null) infoCache = new InfoCache();
                }
                catch (Exception ex)
                {
                    File.Delete(SavePath);
                }
            }
        }

        public static void SaveList()
        {
            string convertedList = JsonConvert.SerializeObject(infoCache);
            File.WriteAllText(SavePath, convertedList);
        }

        public static void DeleteCacheFile()
        {
            File.Delete(SavePath);
        }
    }

    public class InfoCache
    {
        //A dictionary of the spoil subscriber info (guild ids / the channel to output to in them)
        public Dictionary<ulong, ulong> SubsGuildXChannel = new Dictionary<ulong, ulong>();
        //A dictionary of the spoil url and last seen info (different spoil page urls / list of seen spoiler urls latest first)
        public Dictionary<string, List<string>> SpoilUrlsXSeen = new Dictionary<string,List<string>>();
        //interval for spoilers
        public int SpoilerIntervalSeconds = 300;
        //admin users
        public List<ulong> Admins = new List<ulong>();
    }
}
