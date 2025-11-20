using ProyectoNET.Carreras.API.Models;
using NetTopologySuite.Geometries;

namespace ProyectoNET.Carreras.API.Services
{
    public interface IGeoProcessingService
    {
        Task<(Dictionary<int, double> CheckpointsKm, double TotalDistanceKm)> CalculateCheckpointsAndDistanceAsync(
            string geoJsonContent, 
            int carreraId);
        }
}