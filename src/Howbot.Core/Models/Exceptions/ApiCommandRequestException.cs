using System;

namespace Howbot.Core.Models.Exceptions;

public class ApiCommandRequestException(string message) : Exception(message);
