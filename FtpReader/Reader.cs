using Reader.Core;
using CoreFtp;
using System;
using System.Threading.Tasks;
using Renci.SshNet;
using System.IO;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using System.Linq;
using System.Collections.Generic;

namespace FtpReader
{
    public class Reader<TBroadcaster> : IConsumer<TBroadcaster>
    {
        private readonly IStreamBroadcaster<TBroadcaster> _broadCaster;
        public Reader(IStreamBroadcaster<TBroadcaster> broadcaster)
        {
            _broadCaster = broadcaster;
        }

        public async Task Read(ConsumerArgs args, TBroadcaster broadcasterArgs)
        {
            var ftpConsumerArgs = (FtpConsumerArgs)args;

            // TODO: both these functions would ideally return a fileStream
            // to then broadcast in this method, rather than in ReadFTP or ReadSFTP
            if (ftpConsumerArgs.Protocol == ProtocolEnum.FTP)
                await ReadFTP(broadcasterArgs, ftpConsumerArgs);
            else
                await ReadSFTP(broadcasterArgs, ftpConsumerArgs);
        }

        /// <summary>
        /// Use Renci.SshNet.Sftp to read the FTP and then broadcast
        /// </summary>
        /// <param name="broadcasterArgs"></param>
        /// <param name="ftpConsumerArgs"></param>
        /// <returns></returns>
        private async Task ReadSFTP(TBroadcaster broadcasterArgs, FtpConsumerArgs ftpConsumerArgs)
        {
            SftpClient client = null;

            switch (ftpConsumerArgs.AuthType)
            {
                case AuthorisationTypeEnum.Basic:
                    client = new SftpClient(ftpConsumerArgs.Host, ftpConsumerArgs.Username, ftpConsumerArgs.Password);
                    break;
                case AuthorisationTypeEnum.PublicPrivatekey:
                    client = new SftpClient(ftpConsumerArgs.Host, ftpConsumerArgs.Username, new PrivateKeyFile[] { new PrivateKeyFile(ftpConsumerArgs.PrivateKeyFilePath) });
                    break;
                default:
                    throw new NotImplementedException();
            }

            client.Connect();

            if (!string.IsNullOrWhiteSpace(ftpConsumerArgs.Filter))
                throw new NotImplementedException("Filtering not implemented for SFTP");

            var allFiles = client.ListDirectory(ftpConsumerArgs.RemotePath);

            foreach (var filename in ftpConsumerArgs.Filenames)
            {
                List<string> parts = filename.Split("*").ToList();
                var files = allFiles
                    .Where(f => parts.All(p => f.Name.Contains(p, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrWhiteSpace(ftpConsumerArgs.Exclude) || !f.Name.Contains(ftpConsumerArgs.Exclude))))
                    .OrderByDescending(f => f.LastWriteTime)
                    .AsEnumerable();

                if (ftpConsumerArgs.GetLatest)
                {
                    files = files.Take(1);
                }

                foreach (var file in files)
                {
                    try
                    {
                        SetOutputFileNameIfFileBroadcast(broadcasterArgs, file.Name);

                        using (SftpFileStream dataStream = client.OpenRead(Path.Combine(ftpConsumerArgs.RemotePath, file.Name)))
                            await _broadCaster.Broadcast(dataStream, broadcasterArgs);
                    }
                    catch (SftpPathNotFoundException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }

            client.Disconnect();
            client.Dispose();
        }

        /// <summary>
        /// User CoreFtp to read the FTP server and then pass the resulting stream to the broadcaster
        /// </summary>
        /// <param name="broadcasterArgs"></param>
        /// <param name="ftpConsumerArgs"></param>
        /// <returns></returns>
        public async Task ReadFTP(TBroadcaster broadcasterArgs, FtpConsumerArgs ftpConsumerArgs)
        {
            switch (ftpConsumerArgs.AuthType)
            {
                case AuthorisationTypeEnum.Basic:                    
                    break;
                default:
                    throw new NotImplementedException();
            }

            using (var client = new FtpClient(new FtpClientConfiguration
            {
                Host = ftpConsumerArgs.Host,
                Username = ftpConsumerArgs.Username,
                Password = ftpConsumerArgs.Password,
                BaseDirectory = ftpConsumerArgs.RemotePath
            }))
            {
                await client.LoginAsync();
                
                var allFiles = await client.ListFilesAsync();

                var files = allFiles
                    .Where(f => f.Name.ToLower().Contains(ftpConsumerArgs.Filter))
                    .OrderByDescending(f => f.DateModified)
                    .AsEnumerable();

                if (ftpConsumerArgs.GetLatest)
                {
                    files = files.Take(1);
                }

                foreach (var file in files)
                {
                    SetOutputFileNameIfFileBroadcast(broadcasterArgs, file.Name);

                    try
                    {
                        using (var ftpReadStream = await client.OpenFileReadStreamAsync(Path.Combine(ftpConsumerArgs.RemotePath, file.Name)))
                            await _broadCaster.Broadcast(ftpReadStream, broadcasterArgs);
                    }
                    catch (FileNotFoundException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// The Broadcaster needs to know the filename if it is a FileBroadcaster
        /// We can't access FileBroadcasterArgs from the Broadcaster as these are set at top level in the client
        /// TODO: How can we get the two different types of Streams, Renci SSH and Core FTP to convert to a FileStream,
        /// without loading all the file into memory? If we could do this, we could create a file stream in this class
        /// and pass that to the FileBroadcaster
        /// </summary>
        /// <param name="broadcasterArgs"></param>
        /// <param name="file"></param>
        private static void SetOutputFileNameIfFileBroadcast(TBroadcaster broadcasterArgs, string filename)
        {
            switch (typeof(TBroadcaster).Name)
            {
                case "FileBroadcastArgs":
                    broadcasterArgs.GetType().GetProperty("FileName").SetValue(broadcasterArgs, filename);
                    break;
            }
        }
    }
}
