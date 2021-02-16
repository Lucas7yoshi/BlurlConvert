using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;

namespace blurl
{
    class Program
    {
        static void Main(string[] args)
        {
            var filepath = args[0];

            if (filepath.ToLower().EndsWith(".json"))
            {
                var file = File.ReadAllBytes(filepath);
                var file2 = File.OpenRead(filepath);
                //file2.Position += 8;
                using (var t = new BinaryWriter(File.OpenWrite(Path.ChangeExtension(filepath, ".blurl"))))
                {
                    //t.SetLength(0);
                    byte[] magic = { 0x62, 0x6C, 0x75, 0x6C };
                    t.Write(magic);
                    t.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(file.Length)));
                    t.Write(Compress(file2));
                }
                return;
            }
            else if (filepath.ToLower().EndsWith(".blurl"))
            {
                Console.WriteLine("Decompressing...");
                byte[] ret;
                {
                    var fs = File.OpenRead(args[0]);
                    fs.Position = 8;
                    ret = Decompress(fs);
                }
                File.WriteAllText(Path.ChangeExtension(args[0], "json"), JsonConvert.SerializeObject(JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ret)), Formatting.Indented));
                
                //open with default json assigned program
                
                var p = new Process();
                p.StartInfo.FileName = @"explorer";
                p.StartInfo.Arguments = "\"" + Path.ChangeExtension(args[0], "json") + "\"";
                p.Start();
            }
        }

        static byte[] Decompress(Stream data)
        {
            var outputStream = new MemoryStream();
            using (var inputStream = new InflaterInputStream(data))
            {
                inputStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
        static byte[] Compress(Stream data)
        {
            var outputStream = new MemoryStream();
            using (var inputStream = new DeflaterOutputStream(outputStream))
            {
                data.CopyTo(inputStream);
            }
            return outputStream.ToArray();
        }
    }
}