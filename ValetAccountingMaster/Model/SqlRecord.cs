using Microsoft.VisualBasic;
using SQLite;

namespace ValetAccountingMaster.Model
{
    public class SqlRecord : Record
    {
        [PrimaryKey,AutoIncrement]
        public int Key { get; set; }
        public new SqlRecord Clone() => MemberwiseClone() as SqlRecord;

        public (bool IsValid, string? ErrorMsg) Validate()
        {
            if (!(Date is DateTime))
            {
                return (false, $"{nameof(Date)} should be entered.");
            }
            return (true, null);
        }
    }
}
