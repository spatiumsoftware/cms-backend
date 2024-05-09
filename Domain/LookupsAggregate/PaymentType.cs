using Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities.SystemKeys;
namespace Domain.LookupsAggregate
{
    [Table("PaymentTypes", Schema = DbSchemaKeys.Lookup)]
    public class PaymentType :LookupBase
    {
    }
}
