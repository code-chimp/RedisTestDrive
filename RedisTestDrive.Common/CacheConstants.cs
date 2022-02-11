namespace RedisTestDrive.Common;

public static class CacheConstants
{
    // Indicate Database Usage
    public const int GlobalValuesDb = 0;
    public const int StringValuesDb = 1;
    public const int HashValuesDb = 2;
    public const int ListValuesDb = 3;
    public const int SetValuesDb = 4;
    public const int GeoValuesDb = 5;

    // Global Database Keys
    public const string GlobalSampleKey = "global:sample:value";
}
