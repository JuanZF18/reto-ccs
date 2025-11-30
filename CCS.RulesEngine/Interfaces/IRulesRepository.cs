using CCS.RulesEngine.Models;

namespace CCS.RulesEngine.Interfaces;

public interface IRulesRepository
{
    Task LoadRulesAsync();
    List<RuleConfigDbModel> GetRulesForVehicle(string plate);
}