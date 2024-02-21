using System;

namespace Howbot.Core.Models.Exceptions;

public abstract class DiscordLoginException(string message) : Exception(message);
