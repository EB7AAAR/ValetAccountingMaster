namespace ValetAccountingMaster.Model
{
    public class MonthRecord
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public int Workers { get; set; }
        public int NumOfRecords { get; set; }
        public double Income { get; set; }
        public double Tip { get; set; }
        public double DailyExp { get; set; }
        public double DailyNet { get; set; }
        public bool IsClosed { get; set; }
        public MonthRecord Clone() => MemberwiseClone() as MonthRecord;
    }

}
