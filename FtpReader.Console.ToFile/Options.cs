using Reader.Core;
using CommandLine;
using System.Collections.Generic;

namespace FtpReader.Console
{
    public class Options
    {
        [Option('h', "host", Required = true,
          HelpText = "Host.")]
        public string Host { get; set; }

        [Option('o', "output", Required = false,
          HelpText = "CSV output path.")]
        public string OutputPath { get; set; }

        [Option('r', "remote", Required = false,
          HelpText = "Remote path.")]
        public string RemotePath { get; set; }

        [Option('k', "privatekeypath", Required = false,
          HelpText = "Private Key path.")]
        public string PrivateKeyPath { get; set; }

        [Option('f', "filenames", Required = false,
          HelpText = "List of filenames.")]
        public IEnumerable<string> Filenames { get; set; }

        [Option('i', "filter", Default = "",
          HelpText = "Filter by this string.")]
        public string Filter { get; set; }

        [Option('x', "exclude", Default = "",
          HelpText = "Exclude this string.")]
        public string Exclude { get; set; }

        [Option('l', "getLatest", Default = false,
          HelpText = "Only get the latest file.")]
        public bool GetLatest { get; set; }

        [Option('u', "username", Required = false,
          HelpText = "Username.")]
        public string Username { get; set; }

        [Option('p', "password", Required = false,
          HelpText = "Password.")]
        public string Password { get; set; }

        [Option('e', "passwordEnvironmentVariable", Required = false,
          HelpText = "The environment variable where the password is stored")]
        public string PasswordEnvironmentVariable { get; set; }

        [Option('a', "authorisation", Default = AuthorisationTypeEnum.None,
          HelpText = "The type of authorisation")]
        public AuthorisationTypeEnum AuthType { get; set; }

        [Option('c', "protocol", Default = ProtocolEnum.FTP,
          HelpText = "The protocol used to communicate")]
        public ProtocolEnum Protocol { get; set; }
    }
}
