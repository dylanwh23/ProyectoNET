// ProyectoNET.Carreras.API/Sagas/CarreraSagaState.cs

using MassTransit;

namespace ProyectoNET.Carreras.API.Sagas;

// Objeto que se guarda en Redis
public class CarreraSagaState : SagaStateMachineInstance, ISagaVersion
{
    // Requerido por MassTransit
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    
    public int IdCarrera { get; set; }

    public DateTime? FechaInicio { get; set; }

    // El "Snapshot": el último estado de cada corredor
    public int Version { get; set; }

}

// DTO simple para el diccionario
public class CorredorEstadoActual
{
    public int IdCorredor { get; set; }
    public int UltimoPuntoDeControlId { get; set; }
    public double VelocidadKmh { get; set; }
    // Nota: Tu 'CorredorData' no tiene 'Tiempo'. 
    // Si lo agregas en el simulador, añádelo aquí.
}