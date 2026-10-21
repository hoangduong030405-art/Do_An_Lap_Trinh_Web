using Dapper;
using System;
using System.Data;
using WebBanHang.CoreBusiness.Entities;

namespace WebBanHang.Plugins.DataStore.SQL
{
    public class InvoiceStatusTypeHandler : SqlMapper.TypeHandler<InvoiceStatus>
    {
        public override void SetValue(IDbDataParameter parameter, InvoiceStatus value)
        {
            parameter.Value = value.ToString();
        }

        public override InvoiceStatus Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                return InvoiceStatus.Unpaid;
            }

            var str = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(str))
            {
                return InvoiceStatus.Unpaid;
            }

            if (int.TryParse(str, out int intVal))
            {
                return Enum.IsDefined(typeof(InvoiceStatus), intVal) ? (InvoiceStatus)intVal : InvoiceStatus.Unpaid;
            }

            if (string.Equals(str, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceStatus.Unpaid;
            }

            if (Enum.TryParse<InvoiceStatus>(str, true, out var result))
            {
                return result;
            }

            return InvoiceStatus.Unpaid;
        }
    }
}
