namespace PlayerPerformance.Domain.Imports;

public enum ImportType
{
    PLAYER_ROSTER,
    MATCH_PLAYER_STATISTICS,
    MATCH_GPS,
    TRAINING_GPS
}
public enum ImportSourceSystem
{
    GENERIC,
    GPEXE,
    ZONE14,
    OTHER
}
public enum ImportFileFormat
{
    CSV,
    XLSX
}
public enum ImportJobStatus
{
    UPLOADED,
    PARSING,
    VALIDATION_FAILED,
    READY_TO_CONFIRM,
    IMPORTED,
    FAILED, CANCELLED
}
public enum ImportProcessingOperation
{
    PREVIEW,
    VALIDATION,
    CONFIRMATION
}
public enum ImportValidationSeverity
{
    ERROR,
    WARNING
}
public enum ImportAllowedAction
{
    PREVIEW,
    VALIDATE,
    CONFIRM,
    CANCEL
}
