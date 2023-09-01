using SQLite;

namespace ValetAccountingMaster.Model
{
    public class SqlMonthRecord : MonthRecord
    {
        [PrimaryKey,AutoIncrement]
        public int Key { get; set; }
        public new SqlMonthRecord Clone() => MemberwiseClone() as SqlMonthRecord;

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
