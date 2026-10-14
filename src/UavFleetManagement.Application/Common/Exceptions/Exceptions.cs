namespace UavFleetManagement.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"Сущность '{name}' с ключом '{key}' не найдена.") { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Не авторизован.") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Доступ запрещен.") : base(message) { }
}
