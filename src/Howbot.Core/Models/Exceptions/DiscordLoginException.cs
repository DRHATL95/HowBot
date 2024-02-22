using System;

namespace Howbot.Core.Models.Exceptions;

public class DiscordLoginException(string message) : Exception(message);
