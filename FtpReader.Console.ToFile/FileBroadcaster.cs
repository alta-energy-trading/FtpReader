using Reader.Core;
using System.IO;
using System.Threading.Tasks;

namespace FtpReader.Console.ToFile
{
    public class FileBroadcaster : IStreamBroadcaster<FileBroadcastArgs>
    {
        public Task Broadcast(Stream stream, FileBroadcastArgs args)
        {
            using (var fileStream = File.Create(args.OutputPath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return Task.CompletedTask;
        }
    }
}
