namespace CCS.RulesEngine.Models
{
    public class RuleConfigDbModel
    {
        public string vehicle_plate { get; set; } = "";
        public int rule_type { get; set; }
        public string threshold_value { get; set; } = "";
    }
}
