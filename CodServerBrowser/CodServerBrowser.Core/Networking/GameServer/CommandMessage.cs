﻿using System.Diagnostics.CodeAnalysis;

namespace CodServerBrowser.Core.Networking.GameServer
{
    public readonly record struct CommandMessage
    {
        [SetsRequiredMembers]
        public CommandMessage(string commandName, string data = "", char separator = ' ')
        {
            CommandName = commandName;
            Data = data;
            Separator = separator;
        }

        public CommandMessage()
        {
            Data = "";
            Separator = ' ';
        }

        /// <summary>
        /// Name of the command in the message, e.g. 'getinfo' or 'infoResponse'
        /// </summary>
        public required string CommandName { get; init; }

        public string Data { get; init; }

        public char Separator { get; init; }
    }
}