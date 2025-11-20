// ProyectoNET.Carreras.API/Services/GeoProcessingService.cs

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing; // Necesario para LengthIndexedLine
using System.Linq; 

namespace ProyectoNET.Carreras.API.Services
{
    public class GeoProcessingService : IGeoProcessingService
    {
        private record CalculatedCheckpoint(double Km, double Longitud, double Latitud);

        
        public Task<(Dictionary<int, double> CheckpointsKm, double TotalDistanceKm)> CalculateCheckpointsAndDistanceAsync(
            string geoJsonContent, 
            int carreraId)
        {
            var geoReader = new GeoJsonReader();
            
            NetTopologySuite.Features.FeatureCollection featureCollection;
            try
            {
                featureCollection = geoReader.Read<NetTopologySuite.Features.FeatureCollection>(geoJsonContent);
            }
            catch
            {
                throw new ArgumentException("El contenido no es un GeoJSON válido (FeatureCollection esperado).");
            }
            
            // 1. EXTRAER LA RUTA Y LOS PUNTOS
            var routeFeature = featureCollection.FirstOrDefault(f => f.Geometry is LineString);
            LineString? routeLine = routeFeature?.Geometry as LineString;
            var pointFeatures = featureCollection.Where(f => f.Geometry is Point).ToList();

            if (routeLine == null) throw new ArgumentException("GeoJSON debe contener una geometría LineString (la ruta).");
            if (pointFeatures.Count == 0) throw new ArgumentException("GeoJSON debe contener Puntos (checkpoints).");
            
            
            // 2. LEER DISTANCIA TOTAL
            double distanciaTotalKm;
            var attributes = routeFeature!.Attributes; 
            
            if (attributes == null || !attributes.Exists("totalKm")) 
                throw new ArgumentException("El Feature LineString (la ruta) debe incluir una propiedad 'totalKm'.");
            
            if (!double.TryParse(attributes["totalKm"]?.ToString(), out distanciaTotalKm) || distanciaTotalKm <= 0)
                throw new ArgumentException($"La propiedad 'totalKm' debe ser un valor numérico positivo.");
            
            
            // 3. CÁLCULO DE KM DE CADA CHECKPOINT USANDO PROYECCIÓN MANUAL
            
            var rutaIndexada = new LengthIndexedLine(routeLine);
            double longitudTotalNts = rutaIndexada.EndIndex; 
            
            var checkpointsCalculados = new List<CalculatedCheckpoint>();
            var routeCoords = routeLine.Coordinates;

            foreach (var feature in pointFeatures)
            {
                var point = (Point)feature.Geometry;
                Coordinate checkpointCoord = point.Coordinate;
                
                double minDistanceSq = double.MaxValue;
                Coordinate closestPointOnLine = null!;

                // ⚠️ Iterar sobre cada segmento de la LineString y encontrar el punto más cercano
                for (int i = 0; i < routeCoords.Length - 1; i++)
                {
                    Coordinate a = routeCoords[i];
                    Coordinate b = routeCoords[i + 1];

                    // ✅ Proyección manual (usa solo Coordinate y System.Math)
                    Coordinate currentClosest = GetClosestPointOnSegment(checkpointCoord, a, b);
                    double currentDistanceSq = GetDistanceSq(checkpointCoord, currentClosest);

                    if (currentDistanceSq < minDistanceSq)
                    {
                        minDistanceSq = currentDistanceSq;
                        closestPointOnLine = currentClosest;
                    }
                }
                
                if (closestPointOnLine == null)
                {
                    // Esto solo ocurriría con una LineString inválida (ej. sin coordenadas)
                     throw new InvalidOperationException("No se pudo encontrar el punto más cercano en la ruta.");
                }

                // Obtener el índice NTS del punto proyectado
                double indiceNts = rutaIndexada.IndexOf(closestPointOnLine);
                
                // Aplicar la Regla de Tres
                double kmCalculado = (indiceNts / longitudTotalNts) * distanciaTotalKm;

                checkpointsCalculados.Add(new CalculatedCheckpoint(
                    Km: kmCalculado, 
                    Longitud: closestPointOnLine.X, 
                    Latitud: closestPointOnLine.Y 
                ));
            }

            // 4. TRANSFORMACIÓN FINAL: Mapear a Dictionary<int, double>
            var checkpointsKmDictionary = checkpointsCalculados
                .OrderBy(cp => cp.Km) 
                .Select((cp, index) => new { Index = index + 1, cp.Km }) 
                .ToDictionary(item => item.Index, item => item.Km); 

            return Task.FromResult((checkpointsKmDictionary, distanciaTotalKm));
        }

        // ⚠️ Método Auxiliar: Proyección de un Punto P sobre un Segmento AB (Implementación Matemática)
        private static Coordinate GetClosestPointOnSegment(Coordinate p, Coordinate a, Coordinate b)
        {
            // Vector AB (v)
            double vx = b.X - a.X;
            double vy = b.Y - a.Y;
            
            // Longitud cuadrada de AB
            double lengthSq = vx * vx + vy * vy;

            // Si A y B son el mismo punto, la distancia es al punto A
            if (lengthSq == 0.0) return a; 

            // Vector AP (w)
            double wx = p.X - a.X;
            double wy = p.Y - a.Y;

            // Parámetro t = (w . v) / |v|^2
            double dotProduct = wx * vx + wy * vy;
            double t = dotProduct / lengthSq;

            // Clamp t to [0, 1] (Clamping asegura que el punto cae *dentro* del segmento)
            if (t < 0.0)
            {
                return a; // Punto más cercano es A
            }
            if (t > 1.0)
            {
                return b; // Punto más cercano es B
            }

            // Punto más cercano C = A + t * (B - A)
            double cx = a.X + t * vx;
            double cy = a.Y + t * vy;
            
            return new Coordinate(cx, cy);
        }

        // ⚠️ Método Auxiliar: Distancia Euclidiana Cuadrada (para comparar distancias sin sacar raíz cuadrada)
        private static double GetDistanceSq(Coordinate c1, Coordinate c2)
        {
            double dx = c1.X - c2.X;
            double dy = c1.Y - c2.Y;
            return dx * dx + dy * dy;
        }
    }
}