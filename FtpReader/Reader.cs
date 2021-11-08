using Reader.Core;
using FtpReader.Extensions;
using System;
using System.Threading.Tasks;
using Renci.SshNet;
using System.IO;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;

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

            if (ftpConsumerArgs.Protocol == ProtocolEnum.FTP)
                throw new NotImplementedException("FTP");

            SftpClient client = null;

            switch(ftpConsumerArgs.AuthType)
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

            foreach(var filename in ftpConsumerArgs.Filenames)
                try
                {
                    using (SftpFileStream dataStream = client.OpenRead(Path.Combine(ftpConsumerArgs.RemotePath, filename)))
                        await _broadCaster.Broadcast(dataStream, broadcasterArgs);
                }
                catch (SftpPathNotFoundException e)
                {
                    throw new Exception(e.Message);
                }

            client.Disconnect();
            client.Dispose();
        }
    }
}
