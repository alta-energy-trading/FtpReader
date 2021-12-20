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
                try
                {
                    SftpFileStream fileStream = (SftpFileStream)stream;

                    var fileName = fileStream.Name.Split("/").Last();

                    filePath = Path.Combine(filePath, fileName);
                }
                catch(Exception e)
                {
                    throw e;
                }

                using (var file = File.Create(filePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(file);
                }
            }

            return Task.CompletedTask;
        }
    }
}
