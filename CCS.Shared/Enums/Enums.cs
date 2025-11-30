namespace CCS.Shared.Enums;

public enum VehicleType
{
    Unknown = 0,
    Car = 1,
    Motorcycle = 2,
    Truck = 3
}

public enum RuleType
{
    MaxSpeed = 1,
    Geofence = 2,
    UnplannedStop = 3,
    PanicButton = 4,
    CargoTemperature = 5,
    DoorSensor = 6,
    RestrictedSchedule = 7,
}