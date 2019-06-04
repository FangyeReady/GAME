using UnityEngine;
using System.IO;
using System.Text;

namespace  RPG.Saving
{
    public class SavingSystem : MonoBehaviour
    {
        public static void Save(string file)
        {
            string path = GetSavingFile(file);
            print(path);

            using( FileStream fs = File.Open(path, FileMode.Create))
            {
                byte[] buffer = Encoding.UTF8.GetBytes("hello rpg core~!");
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        public static void Load(string file)
        {
            string path = GetSavingFile(file);
            print(path);

            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                string text = Encoding.UTF8.GetString(buffer);
                print("TEXT:====>" + text);
            }
        }


        private static string GetSavingFile(string file)
        {
            return Path.Combine(Application.persistentDataPath, file);
        }
    }
    
}