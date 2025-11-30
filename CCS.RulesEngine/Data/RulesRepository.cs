using CCS.RulesEngine.Interfaces;
using CCS.RulesEngine.Models;
using Dapper;
using Npgsql;
using System.Collections.Concurrent;

namespace CCS.RulesEngine.Data;

public class RulesRepository: IRulesRepository
{
    private readonly string _connectionString;
    private ConcurrentDictionary<string, List<RuleConfigDbModel>> _rulesCache = new();

    public RulesRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // Se llama al iniciar la app
    public async Task LoadRulesAsync()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        var sql = "SELECT vehicle_plate, rule_type, threshold_value FROM rule_configs WHERE is_active = true";

        var rules = await conn.QueryAsync<RuleConfigDbModel>(sql);

        // Agrupar en memoria
        var grouped = rules.GroupBy(r => r.vehicle_plate)
                           .ToDictionary(g => g.Key, g => g.ToList());

        _rulesCache = new ConcurrentDictionary<string, List<RuleConfigDbModel>>(grouped);

        Console.WriteLine($"[CACHE] {_rulesCache.Count} vehículos cargados con reglas.");
    }

    public List<RuleConfigDbModel> GetRulesForVehicle(string plate)
    {
        return _rulesCache.TryGetValue(plate, out var rules) ? rules : new List<RuleConfigDbModel>();
    }
}