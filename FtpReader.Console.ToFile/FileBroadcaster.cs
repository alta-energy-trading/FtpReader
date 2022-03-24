using Reader.Core;
using Renci.SshNet.Sftp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FtpReader.Console.ToFile
{
    public class FileBroadcaster : IStreamBroadcaster<FileBroadcastArgs>
    {
        public Task Broadcast(Stream stream, FileBroadcastArgs args)
        {
            FileAttributes attr = File.GetAttributes(args.OutputPath);
            string filePath = args.OutputPath;

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {              
                filePath = Path.Combine(filePath, args.FileName);

                using (var file = File.Create(filePath))
                {
                    try
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        
                    }
                    catch(Exception e)
                    {
                        // TODO: Why does CoreFtp (https://github.com/sparkeh9/CoreFTP) not allow Seek?
                        // Safe to ignore error as the streeam is at the beginning
                    }

                    stream.CopyTo(file);
                }
            }

            return Task.CompletedTask;
        }
    }
}
