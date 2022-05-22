using System.Threading.Tasks;

namespace WebApplication1.Service
{
    public static class FileExtension
    {
        public static string getExt(string type)
        {
            var ext = type.Split("/");
            return ext[1];
        }
        public static async Task DeleteFile(string path)
        {
            await Task.Delay(300000);
            if(System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        
    }
}