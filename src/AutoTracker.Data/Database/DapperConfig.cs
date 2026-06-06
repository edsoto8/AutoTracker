using System.Data;
using System.Globalization;
using AutoTracker.Core.Enums;
using Dapper;

namespace AutoTracker.Data.Database;

public static class DapperConfig
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured) return;

        SqlMapper.AddTypeHandler(new EnumTypeHandler<FuelType>());
        SqlMapper.AddTypeHandler(new EnumTypeHandler<ServiceType>());
        SqlMapper.AddTypeHandler(new EnumTypeHandler<ExpenseCategory>());
        SqlMapper.AddTypeHandler(new DateTimeHandler());

        _configured = true;
    }
}

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
        => parameter.Value = value.ToString();

    public override T Parse(object value)
        => Enum.Parse<T>(value.ToString()!);
}

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
        => parameter.Value = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public override DateTime Parse(object value)
        => DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture);
}
