using System.Data;
using Dapper;

namespace StarCorp.Data.Connection;

// Dapper nao converte DateOnly em parametro por padrao; este handler mapeia para DbType.Date.
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}
