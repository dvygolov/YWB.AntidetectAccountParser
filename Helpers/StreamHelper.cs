using System.IO;

namespace YWB.AntidetectAccountParser.Helpers
{
    public static class StreamHelper
    {
        public static byte[] ReadAllBytes(this Stream input)
        {
            byte[] array = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = input.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, count);
                }

                return memoryStream.ToArray();
            }
        }
    }
}
